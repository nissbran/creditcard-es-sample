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
       // private static IProjection Projection = new AccountBalanceInMemoryProjection();
        private static AccountBalanceProjection Projection = new AccountBalanceProjection();
       //private static IProjection Projection2 = new AccountBalanceInMemoryProjection();

        static async Task Main(string[] args)
        {
            var eventStoreSubscriptionConnection = EventStoreConnectionFactory.Create(
                new EventStoreSingleNodeConfiguration(),
                new ConsoleLogger(),
                "admin", "changeit");

            await eventStoreSubscriptionConnection.ConnectAsync();

            var stopWatch = new Stopwatch();
            stopWatch.Start();

//            var subscription = eventStoreSubscriptionConnection.SubscribeToAllFrom(null,
//                new CatchUpSubscriptionSettings(10000, 4000, false, false), EventAppearedAll);

            eventStoreSubscriptionConnection.SubscribeToStreamFrom("$ce-Account", null,
                new CatchUpSubscriptionSettings(10000, 1000, false, true),
                EventAppearedAll);
            
//            eventStoreSubscriptionConnection.SubscribeToStreamFrom("$ce-Account", null,
//                new CatchUpSubscriptionSettings(10000, 500, false, true),
//                EventQueued);

            //var subscriptionAsync = await eventStoreSubscriptionConnection.SubscribeToAllAsync(false,
            //    EventAppearedAllAsync,
            //    //    (s, e) =>
            //    //{
            //    //    Console.WriteLine("Test");
            //    //    return Task.CompletedTask;
            //    //},
            //    (s, r, e) =>
            //    {
            //        Console.WriteLine("" + r + e);
            //    });

            Console.ReadLine();

            //Projection.PrintState();
            //Console.WriteLine($"time: {stopWatch.ElapsedMilliseconds} ms, count: {_count}");

            Console.ReadLine();
        }

        private static Task EventAppearedAll(EventStoreCatchUpSubscription eventStoreSubscription, ResolvedEvent resolvedEvent)
        {
            Projection.QueueEvent(resolvedEvent);
            
            return Task.CompletedTask;
        }

        private static async Task EventAppearedAllAsync(EventStoreSubscription eventStoreSubscription, ResolvedEvent resolvedEvent)
        {
            //await Projection.ProcessEvent(resolvedEvent);
        }
    }
}
