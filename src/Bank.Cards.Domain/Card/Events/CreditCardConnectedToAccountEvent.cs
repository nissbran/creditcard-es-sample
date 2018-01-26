namespace Bank.Cards.Domain.Card.Events
{
    using System;
    using Infrastructure.Domain;

    [EventName("CreditCardConnectedToAccount")]
    public class CreditCardConnectedToAccountEvent : CreditCardDomainEvent
    {
        public Guid AccountId { get; set; }
    }
}