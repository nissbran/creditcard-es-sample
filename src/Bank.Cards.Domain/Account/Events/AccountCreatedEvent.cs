namespace Bank.Cards.Domain.Account.Events
{
    using Infrastructure.Domain;

    [EventName("AccountCreated")]
    public class AccountCreatedEvent : AccountDomainEvent
    {
        public string CurrencyIso { get; set; }
    }
}