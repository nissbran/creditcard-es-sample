namespace Bank.Infrastructure.Domain
{
    using System;

    public class DomainMetadata : IDomainMetaData
    {
        public Guid CorrelationId { get; set; }

        public Guid CausationId { get; set; }

        public string StreamId { get; set; }

        public int Version { get; set; }

        public string Schema { get; set; }

        public DateTimeOffset Created { get; set; }
    }
}