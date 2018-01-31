namespace Bank.Infrastructure.Domain
{
    public interface IDomainEvent
    {
        string StreamId { get; set; }

        string Schema { get; }

        IDomainMetaData Metadata { get; set; }
    }
}