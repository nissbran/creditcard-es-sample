namespace Bank.Infrastructure.EventStore
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Domain;

    public interface IEventStore
    {
        Task<IList<IDomainEvent>> GetEventsByStreamId(EventStreamId eventStreamId);

        Task<StreamWriteResult> SaveEvents(EventStreamId eventStreamId, long streamVersion, List<IDomainEvent> events);
    }
}