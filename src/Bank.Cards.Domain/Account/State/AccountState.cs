namespace Bank.Cards.Domain.Account.State
{
    public class AccountState
    {
        public decimal Balance { get; internal set; }

        public long IssuerId { get; internal set; }

        public decimal CreditLimit { get; internal set; }
    }
}