namespace Bank.Cards.Domain.Invoice.Events
{
    using Infrastructure.Domain;

    [EventType("InvoiceSummaryAdded")]
    public class InvoiceSummaryAddedEvent : InvoiceDomainEvent
    {
        public decimal TotalAmountToPay { get; set; }
    }
}