namespace Bank.Cards.Domain.Card.Events
{
    using Infrastructure.Domain;

    [EventName("CardPrinted")]
    public class CreditCardPrintedEvent : CreditCardDomainEvent
    {
        
    }
}