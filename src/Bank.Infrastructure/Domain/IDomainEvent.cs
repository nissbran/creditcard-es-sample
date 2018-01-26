namespace Bank.Infrastructure.Domain
{
    public interface IDomainEvent
    {
        string StreamId { get; set; }

        string Schema { get; }

        DomainMetadata Metadata { get; set; }
    }
}