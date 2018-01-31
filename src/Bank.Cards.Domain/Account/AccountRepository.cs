namespace Bank.Cards.Domain.Account
{
    using System;
    using System.Threading.Tasks;
    using Infrastructure.EventStore;

    public class AccountRepository : IAccountRepository
    {
        private readonly IEventStore _eventStore;

        public AccountRepository(IEventStore eventStore)
        {
            _eventStore = eventStore;
        }

        public async Task<Account> GetAccountById(Guid cardId)
        {
            var domainEvents = await _eventStore.GetEventsByStreamId(new AccountEventStreamId(cardId));

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