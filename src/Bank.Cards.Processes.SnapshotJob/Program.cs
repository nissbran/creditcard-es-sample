namespace Bank.Cards.Processes.SnapshotJob
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading.Tasks;
    using Domain.Account;
    using Domain.Account.Events;
    using Domain.Schemas;
    using EventStore.ClientAPI;
    using EventStore.ClientAPI.Common.Log;
    using EventStore.ClientAPI.SystemData;
    using Infrastructure.Domain;
    using Infrastructure.EventStore;
    using Newtonsoft.Json;
    using Persistence.EventStore.Configuration;

    class Program
    {
        private static Bank.Persistence.EventStore.EventStore _eventStore;
        private static AccountSnapshotRepository _repository;

        static async Task Main(string[] args)
        {
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
                new AccountSchema()
            });
            _repository = new AccountSnapshotRepository(_eventStore);

            try
            {
                await eventStoreSubscriptionConnection.CreatePersistentSubscriptionAsync(
                    "$ce-Account",
                    "Snapshot",
                    PersistentSubscriptionSettings
                        .Create()
                        .StartFromBeginning()
                        .CheckPointAfter(TimeSpan.FromSeconds(5))
                        .ResolveLinkTos()
                        .Build(), new UserCredentials("admin", "changeit"));
            }
            catch (Exception e)
            {
                Console.WriteLine($"Subscription already exist.");
            }

            await eventStoreSubscriptionConnection.ConnectToPersistentSubscriptionAsync("$ce-Account", "Snapshot", EventAppeared);
            await eventStoreSubscriptionConnection.ConnectToPersistentSubscriptionAsync("$ce-Account", "Snapshot", EventAppeared);
            await eventStoreSubscriptionConnection.ConnectToPersistentSubscriptionAsync("$ce-Account", "Snapshot", EventAppeared);
            await eventStoreSubscriptionConnection.ConnectToPersistentSubscriptionAsync("$ce-Account", "Snapshot", EventAppeared);
            await eventStoreSubscriptionConnection.ConnectToPersistentSubscriptionAsync("$ce-Account", "Snapshot", EventAppeared);

            Console.ReadLine();
        }

        private static async Task EventAppeared(EventStorePersistentSubscriptionBase eventStorePersistentSubscriptionBase, ResolvedEvent resolvedEvent)
        {
            if (resolvedEvent.Event.EventNumber > 0 &&
                resolvedEvent.Event.EventNumber % 500 == 0)
            {
                var metaJsonData = Encoding.UTF8.GetString(resolvedEvent.Event.Metadata);
                var eventMetaData = JsonConvert.DeserializeObject<DomainMetadata>(metaJsonData);

                if (eventMetaData.Schema != AccountSchema.SchemaName)
                    return;

                var account = await _repository.GetAccountById(Guid.Parse(eventMetaData.StreamId));

                await _eventStore.SaveSnapshot(new AccountEventStreamId(account.Id), new AccountSnapShot()
                {
                    Balance = account.State.Balance,
                    StreamId = account.Id,
                    SnapshotStreamVersion = account.StreamVersion
                });

                Console.WriteLine($"Account: {eventMetaData.StreamId}, balance: {account.State.Balance}");

                Console.WriteLine($"Event {resolvedEvent.Event.EventNumber}: {resolvedEvent.Event.EventId}");
            }
        }
    }
}
