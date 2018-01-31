namespace Bank.Cards.Domain.Card.Events
{
    using Infrastructure.Domain;

    [EventType("CreditCardDetailsSet")]
    public class CreditCardDetailsSetEvent : CreditCardDomainEvent
    {
        public string NameOnCard { get; set; }
    }
}