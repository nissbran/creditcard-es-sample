namespace Bank.Cards.Processes.ReadProjections.Projections
{
    using System;
    using System.Text;
    using System.Threading.Tasks;
    using Domain.Account;
    using Domain.Account.Events;
    using Domain.Schemas;
    using EventSourcing.Examples.EventStore.ReadProjections.DbContext;
    using EventStore.ClientAPI;
    using Infrastructure.Domain;
    using Microsoft.EntityFrameworkCore;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;

    public class AccountBalanceProjection
    {
        private readonly ReadModelsDbContext _readModelsDbContext;

        private readonly JsonSerializerSettings _jsonSerializerSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };
        private readonly AccountSchema _readSchema = new AccountSchema();

        public AccountBalanceProjection()
        {
            _readModelsDbContext = new ReadModelsDbContext();
            _readModelsDbContext.Database.EnsureCreatedAsync().Wait();
        }

        public async Task ProecessEvent(ResolvedEvent resolvedEvent)
        {
            var accountDomainEvent = ConvertEventDataToAccountDomainEvent(resolvedEvent);
            if (accountDomainEvent == null)
                return;

            var accountId = Guid.Parse(accountDomainEvent.StreamId);
            var balance = await _readModelsDbContext.AccountBalances.SingleOrDefaultAsync(s => s.AccountId == accountId);

            if (balance == null)
            {
                balance = new AccountBalance
                {
                    AccountId = accountId
                };
                await _readModelsDbContext.AccountBalances.AddAsync(balance);
            }

            switch (accountDomainEvent)
            {
                case AccountDebitedEvent debitedEvent:
                    balance.CurrentBalance -= debitedEvent.Amount;
                    break;
                case AccountCreditedEvent creditedEvent:
                    balance.CurrentBalance -= creditedEvent.Amount;
                    break;

            }
            await _readModelsDbContext.SaveChangesAsync();
        }
        
        private AccountDomainEvent ConvertEventDataToAccountDomainEvent(ResolvedEvent resolvedEvent)
        {
            var eventString = Encoding.UTF8.GetString(resolvedEvent.Event.Data);
            var metadataString = Encoding.UTF8.GetString(resolvedEvent.Event.Metadata);

            var metadata = JsonConvert.DeserializeObject<DomainMetadata>(metadataString, _jsonSerializerSettings);

            if (metadata?.Schema == AccountSchema.SchemaName)
            {
                var eventType = _readSchema.GetDomainEventType(resolvedEvent.Event.EventType);

                var accountEvent = (AccountDomainEvent)JsonConvert.DeserializeObject(eventString, eventType, _jsonSerializerSettings);
                accountEvent.StreamId = metadata.StreamId;

                return accountEvent;
            }

            return null;
        }
    }
}