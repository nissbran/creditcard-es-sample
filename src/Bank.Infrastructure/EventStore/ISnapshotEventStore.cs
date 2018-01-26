namespace Bank.Infrastructure.EventStore
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Domain;

    public interface ISnapshotEventStore : IEventStore
    {
        Task<IList<IDomainEvent>> GetEventsBySnapshotStreamId(SnapshotEventStreamId eventStreamId);

        Task<StreamWriteResult> SaveSnapshot(SnapshotEventStreamId snapshotEventStreamId, IDomainEvent snapshot);
    }
}