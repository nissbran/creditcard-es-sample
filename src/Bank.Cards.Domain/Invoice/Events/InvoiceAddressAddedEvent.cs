namespace Bank.Cards.Domain.Invoice.Events
{
    using Infrastructure.Domain;

    [EventName("InvoiceAddressAdded")]
    public class InvoiceAddressAddedEvent : InvoiceDomainEvent
    {
        public string Name { get; set; }

        public string Address { get; set; }

        public string Postalcode { get; set; }

        public string City { get; set; }
    }
}