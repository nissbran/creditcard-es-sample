namespace Bank.Cards.Domain.Account
{
    using Bank.Infrastructure.EventStore;
    using System;
    using System.Threading.Tasks;

    public class AccountSnapshotRepository
    {
        private readonly ISnapshotEventStore _eventStore;

        public AccountSnapshotRepository(ISnapshotEventStore eventStore)
        {
            _eventStore = eventStore;
        }

        public async Task<Account> GetAccountById(Guid cardId)
        {
            var domainEvents = await _eventStore.GetEventsBySnapshotStreamId(new AccountEventStreamId(cardId));
            
            if (domainEvents.Count == 0)
                return null;

            return new Account(domainEvents);
        }

        public async Task SaveAccount(Account account)
        {
            var expectedStreamVersion = account.StreamVersion - account.UncommittedEvents.Count;

            await _eventStore.SaveEvents(new AccountEventStreamId(account.Id), expectedStreamVersion, account.UncommittedEvents);
        }
    }
}
