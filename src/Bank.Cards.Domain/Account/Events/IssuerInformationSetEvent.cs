namespace Bank.Cards.Domain.Account.Events
{
    using Infrastructure.Domain;

    [EventName("IssuerInformationSet")]
    public class IssuerInformationSetEvent : AccountDomainEvent
    {
        public long IssuerId { get; set; }
    }
}