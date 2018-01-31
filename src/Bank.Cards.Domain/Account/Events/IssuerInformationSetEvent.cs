namespace Bank.Cards.Domain.Account.Events
{
    using Infrastructure.Domain;

    [EventType("IssuerInformationSet")]
    public class IssuerInformationSetEvent : AccountDomainEvent
    {
        public long IssuerId { get; set; }
    }
}