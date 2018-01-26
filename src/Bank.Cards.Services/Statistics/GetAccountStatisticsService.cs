namespace Bank.Cards.Services.Statistics
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Domain.Account.Events;
    using Domain.Card.Events;
    using Domain.Statistics;
    using Infrastructure.EventStore;
    using Models;

    public class GetAccountStatisticsService
    {
        private readonly IEventStore _eventStore;

        public GetAccountStatisticsService(IEventStore eventStore)
        {
            _eventStore = eventStore;
        }

        public async Task<IEnumerable<AccountSummary>> GetAccountSummary()
        {
            var accounts = new Dictionary<Guid, AccountSummary>();

            var accountEvents = await _eventStore.GetEventsByStreamId(new StatisticsAllAccountEventStreamId());

            foreach (var accountEvent in accountEvents)
            {
                var accountId = Guid.Parse(accountEvent.StreamId);
                switch (accountEvent)
                {
                    case AccountCreatedEvent accountCreatedEvent:
                        accounts.Add(accountId, new AccountSummary
                        {
                            AccountId = accountId
                        });
                        break;
                    case IssuerInformationSetEvent issuerInformationSetEvent:
                        accounts[accountId].IssuerId = issuerInformationSetEvent.IssuerId;
                        break;
                    case AccountDebitedEvent accountDebited:
                        accounts[accountId].Balance -= accountDebited.Amount;
                        break;
                }
            }

            var cardEvents = await _eventStore.GetEventsByStreamId(new StatisticsAllCardsEventStreamId());

            foreach (var cardEvent in cardEvents)
            {
                switch (cardEvent)
                {
                    case CreditCardConnectedToAccountEvent cardConnectedToAccountEvent:
                        accounts[cardConnectedToAccountEvent.AccountId].NumberOfCards++;
                        break;
                }
            }

            return accounts.Values;
        }

        public async Task<IEnumerable<AccountSummary>> GetAccountSummaryForIssuer(long issuerId)
        {
            var accounts = await GetAccountSummary();

            return accounts.Where(summary => summary.IssuerId == issuerId);
        }
    }
}