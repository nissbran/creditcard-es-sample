namespace Bank.Cards.Domain.Invoice.Events
{
    using Infrastructure.Domain;

    [EventName("InvoiceSummaryAdded")]
    public class InvoiceSummaryAddedEvent : InvoiceDomainEvent
    {
        public decimal TotalAmountToPay { get; set; }
    }
}