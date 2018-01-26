namespace Bank.Cards.Domain.Account.Events
{
    using Infrastructure.Domain;

    [EventName("CreditLimitSet")]
    public class CreditLimitSetEvent : AccountDomainEvent
    {
        public decimal CreditLimit { get; set; }
    }
}