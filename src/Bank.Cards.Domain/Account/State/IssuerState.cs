namespace Bank.Cards.Domain.Account.State
{
    public class IssuerState
    {
        public long IssuerId { get; internal set; }

        public string IssuerName { get; internal set; }
    }
}