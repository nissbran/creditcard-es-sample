namespace Bank.Cards.Processes.ReadProjections.Domain.Account
{
    using System;

    public class AccountBalance
    {
        public Guid AccountId { get; set; }

        public decimal CurrentBalance { get; set; }
    }
}
