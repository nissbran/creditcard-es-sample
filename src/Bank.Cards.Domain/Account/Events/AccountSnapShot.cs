using Bank.Infrastructure.Domain;

namespace Bank.Cards.Domain.Account.Events
{
    [EventName("AccountSnapShot")]
    public class AccountSnapShot : AccountDomainEvent, ISnapshot
    {
        public long SnapshotStreamVersion { get; set; }

        public decimal Balance { get; set; }
    }
}
