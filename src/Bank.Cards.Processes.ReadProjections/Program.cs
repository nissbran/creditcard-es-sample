namespace Bank.Cards.Processes.ReadProjections
{
    using System;
    using System.Diagnostics;
    using System.Threading.Tasks;
    using EventStore.ClientAPI;
    using EventStore.ClientAPI.Common.Log;
    using Persistence.EventStore.Configuration;
    using Projections;

    class Program
    {
        private static AccountBalanceInMemoryProjection Projection = new AccountBalanceInMemoryProjection();

        static async Task Main(string[] args)
        {
            var eventStoreSubscriptionConnection = EventStoreConnectionFactory.Create(
                new EventStoreSingleNodeConfiguration(),
                new ConsoleLogger(),
                "admin", "changeit");

            await eventStoreSubscriptionConnection.ConnectAsync();

            var stopWatch = new Stopwatch();
            stopWatch.Start();

            var subscription = eventStoreSubscriptionConnection.SubscribeToAllFrom(null, 
                new CatchUpSubscriptionSettings(10000, 3000, false, false), EventAppeared);

            Console.ReadLine();

            //Console.WriteLine($"time: {stopWatch.ElapsedMilliseconds} ms, count: {_count}");
            
            Console.ReadLine();
        }

        private static async Task EventAppeared(EventStoreCatchUpSubscription eventStoreSubscription, ResolvedEvent resolvedEvent)
        {
            await Projection.ProcessEvent(resolvedEvent);
        }
    }
}
