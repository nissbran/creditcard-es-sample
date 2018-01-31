namespace Bank.Cards.Domain.Invoice.Events
{
    using Infrastructure.Domain;

    [EventType("InvoiceRowAdded")]
    public class InvoiceRowAddedEvent : InvoiceDomainEvent
    {
        public decimal Amount { get; set; }

        public string Description { get; set; }
    }
}