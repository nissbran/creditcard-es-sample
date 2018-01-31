namespace Bank.Cards.Domain.Invoice.Events
{
    using System;
    using Infrastructure.Domain;

    [EventType("InvoiceCreated")]
    public class InvoiceCreatedEvent : InvoiceDomainEvent
    {
        public Guid InvoiceId { get; set; }

        public Guid AccountId { get; set; }
    }
}