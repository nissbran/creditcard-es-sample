namespace Bank.Cards.Processes.InvoiceProcess
{
    using System;
    using System.Collections.Generic;
    using System.Text;
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

        private static StateDbContext _stateDbContext;

        static async Task Main(string[] args)
        {
            _stateDbContext = new StateDbContext();
            await _stateDbContext.Database.EnsureCreatedAsync();

            var eventStoreSubscriptionConnection = EventStoreConnectionFactory.Create(
               new EventStoreSingleNodeConfiguration(),
               new ConsoleLogger(),
                "admin", "changeit");
            var eventStoreWriteConnection = EventStoreConnectionFactory.Create(
                new EventStoreSingleNodeConfiguration(),
                new ConsoleLogger(),
                "admin", "changeit");

            await eventStoreSubscriptionConnection.ConnectAsync();
            await eventStoreWriteConnection.ConnectAsync();

            _eventStore = new Bank.Persistence.EventStore.EventStore(eventStoreWriteConnection, new List<IEventSchema>
            {
                new InvoiceSchema()
            });
            _repository = new InvoiceRepository(_eventStore);

            try
            {
                await eventStoreSubscriptionConnection.CreatePersistentSubscriptionAsync(
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

            await eventStoreSubscriptionConnection.ConnectToPersistentSubscriptionAsync("$ce-Account", "InvoiceGeneration", EventAppeared);
          
            Console.ReadLine();
        }

        private static async Task EventAppeared(EventStorePersistentSubscriptionBase eventStorePersistentSubscriptionBase, ResolvedEvent resolvedEvent)
        {
            var domainEvent = ConvertEventDataToDomainEvent(resolvedEvent);
            var accountEvent = domainEvent as AccountDomainEvent;

            if (accountEvent == null)
                return;

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
        }

        private static async Task RaiseStateEvent<T>(MonthlyInvoiceStateMachine stateMachine, Event<T> stateEvent, T data) where T : AccountDomainEvent
        {
            var state = await _stateDbContext.MonthlyInvoiceStates.SingleOrDefaultAsync(s => s.AccountId == Guid.Parse(data.StreamId));

            if (state == null)
            {
                state = new MonthlyInvoiceState();
                await _stateDbContext.MonthlyInvoiceStates.AddAsync(state);
            }

            await stateMachine.RaiseEvent(state, stateEvent, data);

            await _stateDbContext.SaveChangesAsync();
        }

        private static IDomainEvent ConvertEventDataToDomainEvent(ResolvedEvent resolvedEvent)
        {
            var eventString = Encoding.UTF8.GetString(resolvedEvent.Event.Data);

            var eventType = _readSchema.GetDomainEventType(resolvedEvent.Event.EventType);

            return (IDomainEvent)JsonConvert.DeserializeObject(eventString, eventType, _jsonSerializerSettings);
        }
    }
}
