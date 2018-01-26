namespace Bank.Cards.Domain.Card
{
    using System.Threading.Tasks;
    using Infrastructure.EventStore;

    public class CreditCardRepository : ICreditCardDomainRepository
    {
        private readonly IEventStore _eventStore;

        public CreditCardRepository(IEventStore eventStore)
        {
            _eventStore = eventStore;
        }

        public async Task<CreditCard> GetCardByHashedPan(string hashedPan)
        {
            var domainEvents = await _eventStore.GetEventsByStreamId(new CreditCardEventStreamId(hashedPan));
            
            if (domainEvents.Count == 0)
                return null;

            return new CreditCard(domainEvents);
        }

        public async Task SaveCard(CreditCard card)
        {
            var streamVersion = card.StreamVersion - card.UncommittedEvents.Count;

            await _eventStore.SaveEvents(new CreditCardEventStreamId(card.Id), streamVersion, card.UncommittedEvents);
        }
    }
}