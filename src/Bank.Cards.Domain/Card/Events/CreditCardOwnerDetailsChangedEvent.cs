namespace Bank.Cards.Domain.Card.Events
{
    using Infrastructure.Domain;

    [EventType("CreditCardOwnerDetailsChanged")]
    public class CreditCardOwnerDetailsChangedEvent : CreditCardDomainEvent
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }
    }
}