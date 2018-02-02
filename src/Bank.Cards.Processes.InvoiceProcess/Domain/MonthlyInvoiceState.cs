namespace Bank.Cards.Processes.InvoiceProcess.Domain
{
    using System;

    public class MonthlyInvoiceState
    {
        public Guid AccountId { get; set; }

        public Guid InvoiceId { get; set; }

        public int CurrentState { get; set; }

        public long AccountStreamVersion { get; set; } = -1;

        public decimal TotalAmountToPay { get; set; }
    }
}
