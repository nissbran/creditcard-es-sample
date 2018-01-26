namespace Bank.Cards.Processes.ReadProjections
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Text;
    using System.Threading.Tasks;
    using Domain.Account;
    using Domain.Account.Events;
    using Domain.Events;
    using Domain.Schemas;
    using EventSourcing.Examples.EventStore.ReadProjections.DbContext;
    using EventStore.ClientAPI;
    using EventStore.ClientAPI.Common.Log;
    using Infrastructure.Domain;
    using Microsoft.EntityFrameworkCore;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;
    using Persistence.EventStore.Configuration;

    class Program
    {
        private static ReadModelsDbContext _readModelsDbContext;
        private static readonly JsonSerializerSettings _jsonSerializerSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };
        private static readonly AccountSchema _readSchema = new AccountSchema();
        private static readonly IDictionary<Guid, AccountBalance> AccountBalances = new Dictionary<Guid, AccountBalance>();
        private static long _count;

        static async Task Main(string[] args)
        {
            _readModelsDbContext = new ReadModelsDbContext();
            await _readModelsDbContext.Database.EnsureCreatedAsync();

            var eventStoreSubscriptionConnection = EventStoreConnectionFactory.Create(
                new EventStoreSingleNodeConfiguration(),
                new ConsoleLogger(),
                "admin", "changeit");

            await eventStoreSubscriptionConnection.ConnectAsync();

            var stopWatch = new Stopwatch();
            stopWatch.Start();

            var subscription = eventStoreSubscriptionConnection.SubscribeToAllFrom(null, CatchUpSubscriptionSettings.Default, EventAppearedInMemory);

            Console.ReadLine();

            Console.WriteLine($"time: {stopWatch.ElapsedMilliseconds} ms, count: {_count}");
            
            Console.ReadLine();
        }

        private static async Task EventAppeared(EventStoreCatchUpSubscription eventStoreSubscription, ResolvedEvent resolvedEvent)
        {
            var accountDomainEvent = ConvertEventDataToAccountDomainEvent(resolvedEvent);
            if (accountDomainEvent == null)
                return;

            var accountId = Guid.Parse(accountDomainEvent.StreamId);
            var balance = await _readModelsDbContext.AccountBalances.SingleOrDefaultAsync(s => s.AccountId == accountId);

            if (balance == null)
            {
                balance = new AccountBalance
                {
                    AccountId = accountId
                };
                await _readModelsDbContext.AccountBalances.AddAsync(balance);
            }

            switch (accountDomainEvent)
            {
                case AccountDebitedEvent debitedEvent:
                    balance.CurrentBalance -= debitedEvent.Amount;
                    break;
                case AccountCreditedEvent creditedEvent:
                    balance.CurrentBalance -= creditedEvent.Amount;
                    break;

            }
            await _readModelsDbContext.SaveChangesAsync();

            _count++;
        }

        private static async Task EventAppearedInMemory(EventStoreCatchUpSubscription eventStoreSubscription, ResolvedEvent resolvedEvent)
        {
            var accountDomainEvent = ConvertEventDataToAccountDomainEvent(resolvedEvent);
            
            if (accountDomainEvent == null)
                return;

            var accountId = Guid.Parse(accountDomainEvent.StreamId);
            AccountBalance balance;

            if (!AccountBalances.TryGetValue(accountId, out balance))
            {
                balance = new AccountBalance
                {
                    AccountId = accountId
                };
                AccountBalances.Add(accountId, balance);
            }

            switch (accountDomainEvent)
            {
                case AccountDebitedEvent debitedEvent:
                    balance.CurrentBalance -= debitedEvent.Amount;
                    break;
                case AccountCreditedEvent creditedEvent:
                    balance.CurrentBalance -= creditedEvent.Amount;
                    break;
            }

            _count++;
        }


        private static AccountDomainEvent ConvertEventDataToAccountDomainEvent(ResolvedEvent resolvedEvent)
        {
            var eventString = Encoding.UTF8.GetString(resolvedEvent.Event.Data);
            var metadataString = Encoding.UTF8.GetString(resolvedEvent.Event.Metadata);

            var metadata = JsonConvert.DeserializeObject<DomainMetadata>(metadataString, _jsonSerializerSettings);

            if (metadata?.Schema == AccountSchema.SchemaName)
            {
                var eventType = _readSchema.GetDomainEventType(resolvedEvent.Event.EventType);

                var accountEvent = (AccountDomainEvent) JsonConvert.DeserializeObject(eventString, eventType, _jsonSerializerSettings);
                accountEvent.StreamId = metadata.StreamId;

                return accountEvent;
            }

            return null;
        }
    }
}
