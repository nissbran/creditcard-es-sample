namespace Bank.Cards.Processes.ReadProjections.Domain.Account.Events
{
    using Infrastructure.Domain;
    using Newtonsoft.Json;
    using Schemas;

    public abstract class AccountDomainEvent : IDomainEvent
    {
        [JsonIgnore]
        public string StreamId { get; set; }

        [JsonIgnore]
        public string Schema => AccountSchema.SchemaName;

        [JsonIgnore]
        public DomainMetadata Metadata { get; set; }
    }
}