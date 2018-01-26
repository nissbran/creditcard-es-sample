﻿namespace Bank.Infrastructure.Domain
{
    using System;

    public class DomainMetadata
    {
        public Guid CorrelationId { get; set; }

        public string StreamId { get; set; }

        public string Schema { get; set; }

        public DateTimeOffset Created { get; set; }
    }
}