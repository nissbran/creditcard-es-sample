namespace Bank.Infrastructure.Domain
{
    using System;

    public interface IDomainMetaData
    {
        Guid CorrelationId { get; set; }

        string StreamId { get; set; }

        int Version { get; set; }

        string Schema { get; set; }

        DateTimeOffset Created { get; set; }
    }
}