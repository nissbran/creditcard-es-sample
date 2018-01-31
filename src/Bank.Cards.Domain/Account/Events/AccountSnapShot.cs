using Bank.Infrastructure.Domain;

namespace Bank.Cards.Domain.Account.Events
{
    [EventType("AccountSnapShot")]
    public class AccountSnapShot : AccountDomainEvent, ISnapshot
    {
        public long SnapshotStreamVersion { get; set; }

        public decimal Balance { get; set; }
    }
}
