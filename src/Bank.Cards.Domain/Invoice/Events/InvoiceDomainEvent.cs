﻿namespace Bank.Cards.Domain.Invoice.Events
{
    using Infrastructure.Domain;
    using Newtonsoft.Json;
    using Schemas;

    public abstract class InvoiceDomainEvent : IDomainEvent
    {
        [JsonIgnore]
        public string StreamId { get; set; }

        [JsonIgnore]
        public int Version { get; set; }

        [JsonIgnore]
        public string Schema => InvoiceSchema.SchemaName;

        [JsonIgnore]
        public IDomainMetaData Metadata { get; set; }
    }
}