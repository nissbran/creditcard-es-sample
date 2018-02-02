namespace Bank.Cards.Domain.Account.Events
{
    using Infrastructure.Domain;

    [EventType("AccountDebited_V2", 1)]
    public class AccountDebitedEvent2 : AccountDomainEvent
    {
        //public decimal Amount { get; set; }

        public decimal AmountExcl { get; set; }

        public decimal VatAmount { get; set; }

        public string Reference { get; set; }

        public string CreatedBy { get; set; }
    }
}