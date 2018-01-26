namespace Bank.Cards.Domain.Invoice.Events
{
    using System;
    using Infrastructure.Domain;

    [EventName("InvoiceCreated")]
    public class InvoiceCreatedEvent : InvoiceDomainEvent
    {
        public Guid InvoiceId { get; set; }

        public Guid AccountId { get; set; }
    }
}