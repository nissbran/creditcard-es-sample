namespace Bank.Cards.Processes.InvoiceProcess
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Automatonymous;
    using Cards.Domain.Account.Events;
    using Cards.Domain.Invoice;
    using Cards.Domain.Schemas;
    using DbContext;
    using Domain;
    using EventStore.ClientAPI;
    using EventStore.ClientAPI.Common.Log;
    using EventStore.ClientAPI.SystemData;
    using Infrastructure.Domain;
    using Infrastructure.EventStore;
    using Microsoft.EntityFrameworkCore;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;
    using Persistence.EventStore.Configuration;
    using Process;

    class Program
    {
        private static Bank.Persistence.EventStore.EventStore _eventStore;
        private static InvoiceRepository _repository;
        private static readonly AccountSchema _readSchema = new AccountSchema();
        private static readonly JsonSerializerSettings _jsonSerializerSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        private static readonly ConcurrentDictionary<Guid, SemaphoreSlim> Locks = new ConcurrentDictionary<Guid, SemaphoreSlim>();

        private static StateDbContext _stateDbContext;
        private static IEventStoreConnection _subscriptionConnection;
        private static IEventStoreConnection _writeConnection;

        static async Task Main(string[] args)
        {
            _stateDbContext = new StateDbContext();
            await _stateDbContext.Database.EnsureCreatedAsync();

            _subscriptionConnection = EventStoreConnectionFactory.Create(
               new EventStoreSingleNodeConfiguration(),
               new ConsoleLogger(),
                "admin", "changeit");
            _writeConnection = EventStoreConnectionFactory.Create(
                new EventStoreSingleNodeConfiguration(),
                new ConsoleLogger(),
                "admin", "changeit");

            await _subscriptionConnection.ConnectAsync();
            await _writeConnection.ConnectAsync();

            _eventStore = new Bank.Persistence.EventStore.EventStore(_writeConnection, new List<IEventSchema>
            {
                new InvoiceSchema()
            });
            _repository = new InvoiceRepository(_eventStore);

            await StartMultipleSubscriptions();

            //await eventStoreSubscriptionConnection.ConnectToPersistentSubscriptionAsync("$ce-Account", "InvoiceGeneration", EventAppeared);
            //await eventStoreSubscriptionConnection.ConnectToPersistentSubscriptionAsync("$ce-Account", "InvoiceGeneration", EventAppeared);
            //await eventStoreSubscriptionConnection.ConnectToPersistentSubscriptionAsync("$ce-Account", "InvoiceGeneration", EventAppeared);
            //await eventStoreSubscriptionConnection.ConnectToPersistentSubscriptionAsync("$ce-Account", "InvoiceGeneration", EventAppeared);

            Console.ReadLine();
        }

        private static async Task StartMultipleSubscriptions()
        {
            try
            {
                await _subscriptionConnection.CreatePersistentSubscriptionAsync(
                    "$et-AccountCreated",
                    "InvoiceGeneration",
                    PersistentSubscriptionSettings
                        .Create()
                        .StartFromBeginning()
                        .CheckPointAfter(TimeSpan.FromSeconds(5))
                        .ResolveLinkTos()
                        .Build(), new UserCredentials("admin", "changeit"));
            }
            catch (Exception e)
            {
                Console.WriteLine($"Subscription already exist.");
            }

            await _subscriptionConnection.ConnectToPersistentSubscriptionAsync("$et-AccountCreated", "InvoiceGeneration", AccountCreatedEventAppeared);

        }

        private static async Task AccountCreatedEventAppeared(
            EventStorePersistentSubscriptionBase eventStorePersistentSubscriptionBase, ResolvedEvent resolvedEvent)
        {
            var accountEvent = ConvertEventDataToAccountDomainEvent(resolvedEvent);
            if (accountEvent == null)
                return;

            try
            {
                await _subscriptionConnection.CreatePersistentSubscriptionAsync(
                    $"Account-{accountEvent.StreamId}",
                    "InvoiceGeneration",
                    PersistentSubscriptionSettings
                        .Create()
                        .StartFromBeginning()
                        .CheckPointAfter(TimeSpan.FromSeconds(5))
                        .ResolveLinkTos()
                        .Build(), new UserCredentials("admin", "changeit"));
            }
            catch (Exception e)
            {
                Console.WriteLine($"Subscription already exist.");
            }

            await _subscriptionConnection.ConnectToPersistentSubscriptionAsync($"Account-{accountEvent.StreamId}", "InvoiceGeneration", AccountEventAppeared);
        }

        private static async Task StartCategoryGeneration()
        {
            try
            {
                await _subscriptionConnection.CreatePersistentSubscriptionAsync(
                    "$ce-Account",
                    "InvoiceGeneration",
                    PersistentSubscriptionSettings
                        .Create()
                        .StartFromBeginning()
                        .WithNamedConsumerStrategy("Pinned")
                        .CheckPointAfter(TimeSpan.FromSeconds(5))
                        .ResolveLinkTos()
                        .Build(), new UserCredentials("admin", "changeit"));
            }
            catch (Exception e)
            {
                Console.WriteLine($"Subscription already exist.");
            }

            await _subscriptionConnection.ConnectToPersistentSubscriptionAsync("$ce-Account", "InvoiceGeneration", AccountEventAppeared);

        }


        private static async Task AccountEventAppeared(EventStorePersistentSubscriptionBase eventStorePersistentSubscriptionBase, ResolvedEvent resolvedEvent)
        {
            var accountEvent = ConvertEventDataToAccountDomainEvent(resolvedEvent);
            if (accountEvent == null)
                return;

            accountEvent.EventNumber = resolvedEvent.Event.EventNumber;
            //var semaphore = Locks.GetOrAdd(Guid.Parse(accountEvent.StreamId), new SemaphoreSlim(1));

            //await semaphore.WaitAsync();

            var stateMachine = new MonthlyInvoiceStateMachine(_repository);

            switch (accountEvent)
            {
                case AccountCreatedEvent accountCreated:
                    await RaiseStateEvent(stateMachine, stateMachine.AccountCreated, accountCreated);
                    break;
                case AccountDebitedEvent accountDebited:
                    await RaiseStateEvent(stateMachine, stateMachine.AccountDebited, accountDebited);
                    break;
                case MonthlyInvoicePeriodEndedEvent monthlyInvoicePeriodEnded:
                    await RaiseStateEvent(stateMachine, stateMachine.MonthlyInvoicePeriodEnded, monthlyInvoicePeriodEnded);
                    break;
            }

            //     semaphore.Release();
        }

        private static async Task RaiseStateEvent<T>(MonthlyInvoiceStateMachine stateMachine, Event<T> stateEvent, T data) where T : AccountDomainEvent
        {
            using (var context = new StateDbContext())
            {
                var state = await context.MonthlyInvoiceStates.SingleOrDefaultAsync(s => s.AccountId == Guid.Parse(data.StreamId));

                if (state == null)
                {
                    state = new MonthlyInvoiceState();
                    await context.MonthlyInvoiceStates.AddAsync(state);
                }

                if (state.AccountStreamVersion > data.EventNumber)
                    return;

                state.AccountStreamVersion = data.EventNumber;

                await stateMachine.RaiseEvent(state, stateEvent, data);

                await context.SaveChangesAsync();
            }
        }

        private static AccountDomainEvent ConvertEventDataToAccountDomainEvent(ResolvedEvent resolvedEvent)
        {
            var metadataString = Encoding.UTF8.GetString(resolvedEvent.Event.Metadata);
            var eventString = Encoding.UTF8.GetString(resolvedEvent.Event.Data);

            //var metadata = new DomainMetaDataWrapper(metadataString);
            var metadata = JsonConvert.DeserializeObject<DomainMetadata>(metadataString, _jsonSerializerSettings);

            var eventType = _readSchema.GetDomainEventType(resolvedEvent.Event.EventType);

            var domainEvent = (IDomainEvent)JsonConvert.DeserializeObject(eventString, eventType, _jsonSerializerSettings);
            domainEvent.StreamId = metadata.StreamId;
            domainEvent.Version = metadata.Version;

            return (AccountDomainEvent)domainEvent;
        }
    }
}
