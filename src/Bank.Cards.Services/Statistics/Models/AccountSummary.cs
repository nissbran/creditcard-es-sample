namespace Bank.Cards.Services.Statistics.Models
{
    using System;

    public class AccountSummary
    {
        public Guid AccountId { get; set; }

        public int NumberOfCards { get; set; }

        public long IssuerId { get; set; }

        public decimal Balance { get; set; }
    }
}