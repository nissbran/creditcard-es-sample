using System.Collections.Concurrent;
using System.Threading;

namespace Bank.Cards.Processes.ReadProjections.Projections
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Domain.Account;
    using Domain.Account.Events;
    using Domain.Schemas;
    using EventStore.ClientAPI;
    using Infrastructure.Domain;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;
    using Utf8Json.Resolvers;

    public class AccountBalanceInMemoryProjection : IProjection
    {
        private readonly AccountSchema _readSchema = new AccountSchema();

        private readonly ConcurrentDictionary<Guid, AccountBalance> _accountBalances =
            new ConcurrentDictionary<Guid, AccountBalance>();

        private readonly JsonSerializerSettings _jsonSerializerSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        private long _count;

        private const int Buckets = 20;

        private readonly BlockingCollection<ResolvedEvent> _blockingCollection =
            new BlockingCollection<ResolvedEvent>(50000);

        private readonly BlockingCollection<ResolvedEvent>[] _blockingCollections =
            new BlockingCollection<ResolvedEvent>[Buckets];

        public AccountBalanceInMemoryProjection()
        {
            for (int i = 0; i < Buckets; i++)
            {
                _blockingCollections[i] = new BlockingCollection<ResolvedEvent>(5000);
            }

            Utf8Json.JsonSerializer.SetDefaultResolver(StandardResolver.AllowPrivateCamelCase);

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
                var collectionNumber = i;
                Task.Run(() =>
                {
                    while (!_blockingCollections[collectionNumber].IsCompleted)
                    {
                        try
                        {
                            CalculateBalance(_blockingCollections[collectionNumber].Take());
                        }
                        catch (InvalidOperationException e)
                        {
                            Console.WriteLine(e);
                            break;
                        }
                    }
                });
            }

//            Task.Run(() =>   
//            {
//                while (!_blockingCollection.IsCompleted)
//                {
//                    try
//                    {
//                        CalculateBalance(_blockingCollection.Take());
//                    }
//                    catch (InvalidOperationException)
//                    {
//                        break;
//                    }
//                }
//            });
        }

        public Task ProcessEvent(ResolvedEvent resolvedEvent)
        {
            CalculateBalance(resolvedEvent);

            return Task.CompletedTask;
        }

        private void CalculateBalance(ResolvedEvent resolvedEvent)
        {
            Interlocked.Increment(ref _count);

            var accountDomainEvent = ConvertEventDataToAccountDomainEvent(resolvedEvent);

            if (accountDomainEvent == null)
                return;

            var accountId = Guid.Parse(accountDomainEvent.StreamId);

            if (!_accountBalances.TryGetValue(accountId, out var balance))
            {
                balance = new AccountBalance
                {
                    AccountId = accountId,
                    Version = -1
                };
                _accountBalances.TryAdd(accountId, balance);
            }

            Thread.Sleep(1);

            if (balance.Version >= resolvedEvent.Event.EventNumber)
            {
                Console.WriteLine("Duplicate");
                return;
            }

            switch (accountDomainEvent)
            {
                case AccountDebitedEvent debitedEvent:
                    balance.CurrentBalance -= debitedEvent.Amount;
                    if (debitedEvent.Version < 2)
                    {
                        balance.CurrentVatBalance -= 2.5m;
                    }
                    else
                    {
                        balance.CurrentVatBalance -= debitedEvent.VatAmount;
                    }

                    break;
                case AccountDebitedEvent2 debitedEvent2:
                    balance.CurrentBalance -= (debitedEvent2.AmountExcl + debitedEvent2.VatAmount);
                    balance.CurrentVatBalance -= debitedEvent2.VatAmount;
                    break;
                case AccountCreditedEvent creditedEvent:
                    balance.CurrentBalance -= creditedEvent.Amount;
                    break;
            }

            balance.Version = resolvedEvent.Event.EventNumber;
        }

        public void PrintState()
        {
            foreach (var accountBalance in _accountBalances)
            {
                Console.WriteLine(
                    $"Stream: {accountBalance.Key}, Balance: {accountBalance.Value.CurrentBalance}, Vat: {accountBalance.Value.CurrentVatBalance}");
            }
        }

        public void QueueEvent(ResolvedEvent resolvedEvent)
        {
            if (!_readSchema.EventTypeExistInSchema(resolvedEvent.Event.EventType))
                return;

            //var stream = resolvedEvent.Event.EventStreamId.AsSpan();

            //var value = int.Parse(stream.Slice(stream.Length - 1));
            var bucketIndex = (int) ((uint) resolvedEvent.Event.EventStreamId.GetHashCode() % Buckets);

            _blockingCollections[bucketIndex].Add(resolvedEvent);
        }

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