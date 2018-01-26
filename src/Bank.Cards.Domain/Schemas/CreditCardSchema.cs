namespace Bank.Cards.Domain.Schemas
{
    using Card.Events;
    using Infrastructure.EventStore;

    public class CreditCardSchema : EventSchema<CreditCardDomainEvent>
    {
        public const string SchemaName = "CreditCard";

        public override string Name => SchemaName;
    }
}