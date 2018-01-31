namespace Bank.Cards.Processes.ReadProjections.Domain.Card.Events
{
    using System;
    using Infrastructure.Domain;

    [EventType("CreditCardCreated")]
    public class CreditCardCreatedEvent : CreditCardDomainEvent
    {
        public string EncryptedPan { get; set; }

        public string HashedPan { get; set; }

        public DateTimeOffset ExpiryDate { get; set; }
    }
}