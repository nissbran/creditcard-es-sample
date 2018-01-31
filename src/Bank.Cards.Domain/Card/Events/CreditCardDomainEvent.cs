namespace Bank.Cards.Domain.Card.Events
{
    using Infrastructure.Domain;
    using Newtonsoft.Json;
    using Schemas;

    public abstract class CreditCardDomainEvent : IDomainEvent
    {
        [JsonIgnore]
        public string StreamId { get; set; }

        [JsonIgnore]
        public int Version { get; set; }

        [JsonIgnore]
        public string Schema => CreditCardSchema.SchemaName;

        [JsonIgnore]
        public IDomainMetaData Metadata { get; set; }
    }
}