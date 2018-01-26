namespace Bank.Cards.Domain.Invoice
{
    using System;
    using Infrastructure.EventStore;

    public class InvoiceEventStreamId : EventStreamId
    {
        public Guid InvoiceId { get; }

        public override string StreamName => $"Invoice-{InvoiceId}";

        public InvoiceEventStreamId(Guid invoiceId)
        {
            InvoiceId = invoiceId;
        }
    }
}