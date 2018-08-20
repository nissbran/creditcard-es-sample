namespace Bank.Cards.Domain.Account.Events
{
    using Infrastructure.Domain;
    using Newtonsoft.Json;
    using Schemas;
    using System.Runtime.Serialization;

    public abstract class AccountDomainEvent : IDomainEvent
    {
        [IgnoreDataMember]
        public string StreamId { get; set; }

        [IgnoreDataMember]
        public int Version { get; set; }

        [IgnoreDataMember]
        public string Schema => AccountSchema.SchemaName;

        [IgnoreDataMember]
        public IDomainMetaData Metadata { get; set; }

        [IgnoreDataMember]
        public long EventNumber { get; set; }
    }
}