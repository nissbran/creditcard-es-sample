using System.Collections.Generic;

namespace Bank.Persistence.EventStore.Configuration
{
    public interface IEventStoreClusterConfiguration
    {
        bool UseSsl { get; }

        IEnumerable<IEventStoreClusterNode> ClusterNodes { get; }
    }
}