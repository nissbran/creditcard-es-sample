namespace Bank.Cards.Domain.Card.Events
{
    using Infrastructure.Domain;

    [EventName("CreditCardDetailsSet")]
    public class CreditCardDetailsSetEvent : CreditCardDomainEvent
    {
        public string NameOnCard { get; set; }
    }
}