namespace Bank.Cards.Test.EventStore.Write
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;
    using Domain.Account;
    using Domain.Account.Events;
    using Domain.Schemas;
    using global::EventStore.ClientAPI.Common.Log;
    using Infrastructure.EventStore;
    using Persistence.EventStore.Configuration;

    public class Program
    {
        private static Bank.Persistence.EventStore.EventStore _eventStore;
        private static AccountSnapshotRepository _accountSnapshotRepository;

        private static readonly decimal[] Balances = new decimal[10];

        private static ConcurrentBag<long>[] ReadTimings = CreateTimingsArray();

        private static ConcurrentBag<long>[] WriteTimings = CreateTimingsArray();

        private static ConcurrentBag<long>[] CreateTimingsArray()
        {
            return new[]
            {
                new ConcurrentBag<long>(),
                new ConcurrentBag<long>(),
                new ConcurrentBag<long>(),
                new ConcurrentBag<long>(),
                new ConcurrentBag<long>(),
            };
        }

        static async Task Main(string[] args)
        {
            var eventStoreConnection = EventStoreConnectionFactory.Create(
                new EventStoreSingleNodeConfiguration(),
                new ConsoleLogger(),
                "admin", "changeit");

            await eventStoreConnection.ConnectAsync();

            _eventStore = new Bank.Persistence.EventStore.EventStore(eventStoreConnection, new List<IEventSchema>
            {
                new AccountSchema()
            });
            _accountSnapshotRepository = new AccountSnapshotRepository(_eventStore);

            var tasks = new Task[5];

            for (int i = 0; i < 5; i++)
            {
                var number = i;
                tasks[i] = Task.Run(async () => { await CreateStreams(number); });
            }

            await Task.Delay(4000);

            Task.Run(async () =>
            {
                while (true)
                {
                    await Task.Delay(2000);

                    Console.WriteLine("-----Status-----");

                    for (int i = 0; i < 5; i++)
                    {
                        var readTimings = Math.Floor(ReadTimings[i].Average());
                        var writeTimings = Math.Floor(WriteTimings[i].Average());

                        Console.WriteLine($"Stream: {i}, Balance: {Balances[i]}, Read: {readTimings} ms, Write: {writeTimings} ms");
                    }

                    ReadTimings = CreateTimingsArray();
                    WriteTimings = CreateTimingsArray();
                }
            });

            await Task.WhenAll(tasks);
            
            Console.ReadLine();
        }

        private static async Task CreateStreams(int number)
        {
            var id = Guid.Parse($"42a11f29-4578-4d19-b1ec-544260ea401{number}");

            var stopwatch = new Stopwatch();

            for (int i = 0; i <= 2000; i++)
            {
                //var repository2 = new AccountRepository(eventStore);

                stopwatch.Start();

                var account = await _accountSnapshotRepository.GetAccountById(id);
                //var account2 = await repository2.GetAccountById(id);

                ReadTimings[number].Add(stopwatch.ElapsedMilliseconds);
                stopwatch.Restart();

                if (account == null)
                {
                    account = new Account(id);
                }
                else
                {
                    for (int y = 0; y <= 100; y++)
                    {
                        account.AddEvent(new AccountDebitedEvent
                        {
                            Amount = 10,
                            VatAmount = 2.5m
                        });
                    }

                    if (i > 0 &&
                        i % 50 == 0)
                    {
                        account.AddEvent(new MonthlyInvoicePeriodEndedEvent());
                    }
                }

                await _accountSnapshotRepository.SaveAccount(account);

                WriteTimings[number].Add(stopwatch.ElapsedMilliseconds);

                Balances[number] = account.State.Balance;
            }
        }
    }
}
