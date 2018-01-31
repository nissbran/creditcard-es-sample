namespace Bank.Cards.Domain.Card.Events
{
    using System;
    using Infrastructure.Domain;

    [EventType("CreditCardConnectedToAccount")]
    public class CreditCardConnectedToAccountEvent : CreditCardDomainEvent
    {
        public Guid AccountId { get; set; }
    }
}