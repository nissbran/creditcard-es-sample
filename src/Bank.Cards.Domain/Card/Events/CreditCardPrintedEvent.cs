namespace Bank.Cards.Domain.Card.Events
{
    using Infrastructure.Domain;

    [EventType("CardPrinted")]
    public class CreditCardPrintedEvent : CreditCardDomainEvent
    {
        
    }
}