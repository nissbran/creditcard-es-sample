namespace Bank.Cards.Test.SqlStreamStore.Write
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading.Tasks;
    using Domain.Account;
    using Domain.Account.Events;
    using Domain.Schemas;
    using Infrastructure.EventStore;
    using Persistence.Sql;
    using Persistence.Sql.Configuration;

    class Program
    {
        private const string ConnectionString = @"Data Source=(LocalDb)\MSSQLLocalDB;Initial Catalog=EventStoreTest;Integrated Security=SSPI";

        public static async Task Main(string[] args)
        {
            var streamStore = await StreamStoreFactory.InitializeMsSqlStreamStore(ConnectionString);

            var sqlEventStore = new SqlEventStore(streamStore, new List<IEventSchema>
            {
                new AccountSchema()
            });

            var stopwatch = new Stopwatch();

            var id = Guid.Parse("42a11f29-4578-4d19-b1ec-544260ea4001");

            for (int i = 0; i < 50; i++)
            {
                var repository = new AccountRepository(sqlEventStore);

                stopwatch.Start();

                var account = await repository.GetAccountById(id);

                Console.WriteLine($"Read Time: {stopwatch.ElapsedMilliseconds} ms");
                stopwatch.Restart();

                if (account == null)
                {
                    account = new Account(id);
                }
                else
                {
                    for (int y = 0; y < 1000; y++)
                    {
                        account.AddEvent(new AccountDebitedEvent
                        {
                            Amount = 1
                        });
                    }
                }

                await repository.SaveAccount(account);

                Console.WriteLine($"Write Time: {stopwatch.ElapsedMilliseconds} ms");
            }

            Console.ReadLine();
        }
    }
}
