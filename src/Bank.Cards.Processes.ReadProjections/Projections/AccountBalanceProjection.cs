using System.Collections.Concurrent;
using System.Threading;
using Utf8Json.Resolvers;

namespace Bank.Cards.Processes.ReadProjections.Projections
{
    using System;
    using System.Text;
    using System.Threading.Tasks;
    using Domain.Account;
    using Domain.Account.Events;
    using Domain.Schemas;
    using EventSourcing.Examples.EventStore.ReadProjections.DbContext;
    using EventStore.ClientAPI;
    using Infrastructure.Domain;
    using Microsoft.EntityFrameworkCore;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;

    public class AccountBalanceProjection
    {
        private readonly ReadModelsDbContext _readModelsDbContext;

        private readonly JsonSerializerSettings _jsonSerializerSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        private readonly AccountSchema _readSchema = new AccountSchema();

        private const int Buckets = 20;
        private long _count;

        private readonly BlockingCollection<ResolvedEvent>[] _blockingCollections =
            new BlockingCollection<ResolvedEvent>[Buckets];

        public AccountBalanceProjection()
        {
            Utf8Json.JsonSerializer.SetDefaultResolver(StandardResolver.AllowPrivateCamelCase);

            for (int i = 0; i < Buckets; i++)
            {
                _blockingCollections[i] = new BlockingCollection<ResolvedEvent>(5000);
            }

            using (var context = new ReadModelsDbContext())
            {
                context.Database.EnsureCreatedAsync().Wait();
            }

            Task.Run(async () =>
            {
                while (true)
                {
                    await Task.Delay(2000);

                    Console.WriteLine("-----Status-----");
                    Console.WriteLine($"Event count: {Interlocked.Read(ref _count)}");
                    //Console.WriteLine($"Queue count: {_blockingCollection.Count}");
                    Console.WriteLine($"Queue count: {_blockingCollections[0].Count}");
                    Console.WriteLine($"Queue count: {_blockingCollections[1].Count}");
                    Console.WriteLine($"Queue count: {_blockingCollections[2].Count}");
//                    foreach (var accountBalance in _accountBalances)
//                    {
//                        Console.WriteLine($"Stream: {accountBalance.Key}, Balance: {accountBalance.Value.CurrentBalance}, Vat: {accountBalance.Value.CurrentVatBalance}");
//                    }
                }
            });

            for (int i = 0; i < Buckets; i++)
            {
                var bucketIndex = i;
                Task.Run(async () =>
                {
                    while (!_blockingCollections[bucketIndex].IsCompleted)
                    {
                        try
                        {
                            await Calculate(_blockingCollections[bucketIndex].Take());
                        }
                        catch (InvalidOperationException e)
                        {
                            Console.WriteLine(e);
                            break;
                        }
                    }
                });
            }
        }
//
//        public async Task ProcessEvent(ResolvedEvent resolvedEvent)
//        {
//         
//        }

        private async Task Calculate(ResolvedEvent resolvedEvent)
        {
            Interlocked.Increment(ref _count);

            var accountDomainEvent = ConvertEventDataToAccountDomainEvent(resolvedEvent);
            if (accountDomainEvent == null)
                return;
            
            using (var context = new ReadModelsDbContext())
            {
                var accountId = Guid.Parse(accountDomainEvent.StreamId);
                var balance =
                    await context.AccountBalances.SingleOrDefaultAsync(s => s.AccountId == accountId);

                if (balance == null)
                {
                    balance = new AccountBalance
                    {
                        AccountId = accountId,
                        Version = -1
                    };
                    await context.AccountBalances.AddAsync(balance);
                }

                if (balance.Version >= resolvedEvent.Event.EventNumber)
                {
                    return;
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

                balance.Version = resolvedEvent.Event.EventNumber;

                await context.SaveChangesAsync();
            }
        }

        public void QueueEvent(ResolvedEvent resolvedEvent)
        {
            if (!_readSchema.EventTypeExistInSchema(resolvedEvent.Event.EventType))
                return;

            var bucketIndex = (int) ((uint) resolvedEvent.Event.EventStreamId.GetHashCode() % Buckets);

            _blockingCollections[bucketIndex].Add(resolvedEvent);
        }

//        private AccountDomainEvent ConvertEventDataToAccountDomainEvent(ResolvedEvent resolvedEvent)
//        {
//            var eventString = Encoding.UTF8.GetString(resolvedEvent.Event.Data);
//            var metadataString = Encoding.UTF8.GetString(resolvedEvent.Event.Metadata);
//
//            var metadata = JsonConvert.DeserializeObject<DomainMetadata>(metadataString, _jsonSerializerSettings);
//
//            if (metadata?.Schema == AccountSchema.SchemaName)
//            {
//                var eventType = _readSchema.GetDomainEventType(resolvedEvent.Event.EventType);
//
//                var accountEvent =
//                    (AccountDomainEvent) JsonConvert.DeserializeObject(eventString, eventType, _jsonSerializerSettings);
//                accountEvent.StreamId = metadata.StreamId;
//
//                return accountEvent;
//            }
//
//            return null;
//        }

        private AccountDomainEvent ConvertEventDataToAccountDomainEvent(ResolvedEvent resolvedEvent)
        {
            //var metadataString = Encoding.UTF8.GetString(resolvedEvent.Event.Metadata);

            //var metadata = JsonConvert.DeserializeObject<DomainMetadata>(metadataString, _jsonSerializerSettings);

            var metadata = Utf8Json.JsonSerializer.Deserialize<DomainMetadata>(resolvedEvent.Event.Metadata);

            if (metadata?.Schema == AccountSchema.SchemaName)
            {
                var eventType = _readSchema.GetDomainEventType(resolvedEvent.Event.EventType);

                var accountEvent =
                    (AccountDomainEvent) Utf8Json.JsonSerializer.NonGeneric.Deserialize(eventType,
                        resolvedEvent.Event.Data);

                accountEvent.StreamId = metadata.StreamId;
                accountEvent.Version = metadata.Version;

                return accountEvent;
            }

            return null;
        }
    }
}