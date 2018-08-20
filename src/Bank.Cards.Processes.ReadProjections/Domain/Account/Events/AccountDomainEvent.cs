namespace Bank.Cards.Processes.ReadProjections.Domain.Account.Events
{
    using Infrastructure.Domain;
    using Newtonsoft.Json;
    using Schemas;
    using System.Runtime.Serialization;

    public abstract class AccountDomainEvent : IDomainEvent
    {
        [JsonIgnore]
        [IgnoreDataMember]
        public string StreamId { get; set; }

        [JsonIgnore]
        [IgnoreDataMember]
        public int Version { get; set; }

        [JsonIgnore]
        [IgnoreDataMember]
        public string Schema => AccountSchema.SchemaName;

        [JsonIgnore]
        [IgnoreDataMember]
        public IDomainMetaData Metadata { get; set; }
    }
}