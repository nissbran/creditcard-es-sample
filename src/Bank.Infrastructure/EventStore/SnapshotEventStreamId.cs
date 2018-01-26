namespace Bank.Infrastructure.EventStore
{
    public abstract class SnapshotEventStreamId : EventStreamId
    {
        public abstract string SnapshotStreamName { get; }
    }
}