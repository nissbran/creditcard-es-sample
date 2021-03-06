﻿namespace Bank.Cards.Domain.Account.Events
{
    using Infrastructure.Domain;
    using Newtonsoft.Json;
    using Schemas;

    public abstract class AccountDomainEvent : IDomainEvent
    {
        [JsonIgnore]
        public string StreamId { get; set; }

        [JsonIgnore]
        public int Version { get; set; }

        [JsonIgnore]
        public string Schema => AccountSchema.SchemaName;

        [JsonIgnore]
        public IDomainMetaData Metadata { get; set; }

        [JsonIgnore]
        public long EventNumber { get; set; }
    }
}