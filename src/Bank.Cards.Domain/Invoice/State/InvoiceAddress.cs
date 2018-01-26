namespace Bank.Cards.Domain.Invoice.State
{
    public class InvoiceAddress
    {
        public string Name { get; internal set; }
        
        public string Address { get; internal set; }

        public string PostalCode { get; internal set; }

        public string City { get; internal set; }
    }
}
