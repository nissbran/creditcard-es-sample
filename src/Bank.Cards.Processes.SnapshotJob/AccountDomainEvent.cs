using System;

namespace EventSourcing.Examples.EventStore.SnapshotJob
{
    public class AccountDomainEvent2
    {
        public Guid AggregateRootId { get; set; }
    }
}
