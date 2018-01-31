namespace Bank.Cards.Domain.Account.Events
{
    using Infrastructure.Domain;

    [EventType("AccountDebited", 2)]
    public class AccountDebitedEvent : AccountDomainEvent
    {
        public decimal Amount { get; set; }

        public decimal VatAmount { get; set; }

        public string Reference { get; set; }

        public string CreatedBy { get; set; }
    }
}