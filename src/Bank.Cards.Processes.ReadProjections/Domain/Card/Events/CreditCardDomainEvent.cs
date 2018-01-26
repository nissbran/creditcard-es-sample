namespace Bank.Cards.Processes.ReadProjections.Domain.Card.Events
{
    using Infrastructure.Domain;
    using Newtonsoft.Json;
    using Schemas;

    public abstract class CreditCardDomainEvent : IDomainEvent
    {
        [JsonIgnore]
        public string StreamId { get; set; }

        [JsonIgnore]
        public string Schema => CreditCardSchema.SchemaName;

        [JsonIgnore]
        public DomainMetadata Metadata { get; set; }
    }
}