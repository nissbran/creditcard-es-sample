namespace Bank.Cards.Domain.Invoice.State
{
    using System;

    public class InvoiceState
    {
        public Guid AccountId { get; internal set; }

        public InvoiceAddress Address { get; }

        public InvoiceState()
        {
            Address = new InvoiceAddress();
        }
    }
}