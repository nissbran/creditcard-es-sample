namespace Bank.Cards.Processes.SubscriptionTests
{
    using System;
    using System.Diagnostics;
    using System.Threading.Tasks;
    using EventStore.ClientAPI;
    using EventStore.ClientAPI.Common.Log;
    using EventStore.ClientAPI.SystemData;
    using Persistence.EventStore.Configuration;
    using Projections;

    class Program
    {
        private static IProjection Projection = new EventCounterInMemoryProjection();

        public static async Task Main(string[] args)
        {
            var eventStoreSubscriptionConnection = EventStoreConnectionFactory.Create(
                new EventStoreSingleNodeConfiguration(),
                new ConsoleLogger(),
                "admin", "changeit");

            await eventStoreSubscriptionConnection.ConnectAsync();

            try
            {
                await eventStoreSubscriptionConnection.CreatePersistentSubscriptionAsync(
                    "$ce-Account",
                    "Counter",
                    PersistentSubscriptionSettings
                        .Create()
                        .StartFromBeginning()
                        .WithLiveBufferSizeOf(100000)
                        .WithBufferSizeOf(100000)
                        .WithReadBatchOf(4000)
                        .MinimumCheckPointCountOf(10000)
                        .MaximumCheckPointCountOf(100000)
                        .PreferDispatchToSingle()
                        .CheckPointAfter(TimeSpan.FromSeconds(5))
                        .ResolveLinkTos()
                        .Build(), new UserCredentials("admin", "changeit"));
            }
            catch (Exception e)
            {
                Console.WriteLine($"Subscription already exist.");
            }

            await eventStoreSubscriptionConnection.ConnectToPersistentSubscriptionAsync("$ce-Account", "Counter", EventAppeared, bufferSize: 1000);
            await eventStoreSubscriptionConnection.ConnectToPersistentSubscriptionAsync("$ce-Account", "Counter", EventAppeared, bufferSize: 1000);
            await eventStoreSubscriptionConnection.ConnectToPersistentSubscriptionAsync("$ce-Account", "Counter", EventAppeared, bufferSize: 1000);
            await eventStoreSubscriptionConnection.ConnectToPersistentSubscriptionAsync("$ce-Account", "Counter", EventAppeared, bufferSize: 1000);
            await eventStoreSubscriptionConnection.ConnectToPersistentSubscriptionAsync("$ce-Account", "Counter", EventAppeared, bufferSize: 1000);

            Console.ReadLine();
        }

        private static async Task EventAppeared(EventStorePersistentSubscriptionBase eventStorePersistentSubscriptionBase, ResolvedEvent resolvedEvent)
        {
            await Projection.ProcessEvent(resolvedEvent);
        }
    }
}
