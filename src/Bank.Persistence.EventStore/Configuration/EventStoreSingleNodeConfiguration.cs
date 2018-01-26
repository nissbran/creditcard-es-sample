﻿namespace Bank.Persistence.EventStore.Configuration
{
    public class EventStoreSingleNodeConfiguration : IEventStoreConfiguration
    {
        public bool UseSingleNode { get; }
        public string SingleNodeConnectionUri { get; }
        public IEventStoreClusterConfiguration ClusterConfiguration { get; }

        public EventStoreSingleNodeConfiguration()
        {
            UseSingleNode = true;
            SingleNodeConnectionUri = "tcp://localhost:1113";
            ClusterConfiguration = null;
        }
    }
}