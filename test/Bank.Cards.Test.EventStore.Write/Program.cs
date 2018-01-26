namespace Bank.Cards.Test.EventStore.Write
{
    using System;
    using System.Collections.Generic;
    //using System.Collections.Immutable;
    //using System.Collections.Immutable;
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

        private static readonly List<long>[] ReadTimings = new List<long>[]
        {
            new List<long>(),
            new List<long>(),
            new List<long>(),
            new List<long>(),
            new List<long>(),
        };

        private static readonly List<long>[] WriteTimings = new List<long>[]
        {
            new List<long>(),
            new List<long>(),
            new List<long>(),
            new List<long>(),
            new List<long>(),
        };

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
                //var repository = new AccountSnapshotRepository(_eventStore);
                //var repository2 = new AccountRepository(eventStore);

                stopwatch.Start();

                var account = await _accountSnapshotRepository.GetAccountById(id);
                //var account2 = await repository2.GetAccountById(id);

                ReadTimings[number].Add(stopwatch.ElapsedMilliseconds);
                //Console.WriteLine($"Read Time: {stopwatch.ElapsedMilliseconds} ms");
                stopwatch.Restart();

                if (account == null)
                {
                    account = new Account(id);
                }
                else
                {
                    for (int y = 0; y <= 10; y++)
                    {
                        account.AddEvent(new AccountDebitedEvent
                        {
                            Amount = 1
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
                //Console.WriteLine($"Write Time: {stopwatch.ElapsedMilliseconds} ms");
            }
        }
    }
}
