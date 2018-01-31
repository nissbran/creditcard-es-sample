namespace Bank.Cards.Processes.ReadProjections.Projections
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading.Tasks;
    using Domain.Account;
    using Domain.Account.Events;
    using Domain.Schemas;
    using EventStore.ClientAPI;
    using Infrastructure.Domain;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;

    public class AccountBalanceInMemoryProjection : IProjection
    {
        private readonly AccountSchema _readSchema = new AccountSchema();
        private readonly IDictionary<Guid, AccountBalance> _accountBalances = new Dictionary<Guid, AccountBalance>();
        private readonly JsonSerializerSettings _jsonSerializerSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };
        private long _count;

        public AccountBalanceInMemoryProjection()
        {
            Task.Run(async () =>
            {
                while (true)
                {
                    await Task.Delay(2000);

                    Console.WriteLine("-----Status-----");
                    Console.WriteLine($"Event count: {_count}");
                    foreach (var accountBalance in _accountBalances)
                    {
                        Console.WriteLine($"Stream: {accountBalance.Key}, Balance: {accountBalance.Value.CurrentBalance}");
                    }
                }
            });
        }

        public Task ProcessEvent(ResolvedEvent resolvedEvent)
        {
            _count++;

            var accountDomainEvent = ConvertEventDataToAccountDomainEvent(resolvedEvent);

            if (accountDomainEvent == null)
                return Task.CompletedTask;

            var accountId = Guid.Parse(accountDomainEvent.StreamId);

            if (!_accountBalances.TryGetValue(accountId, out var balance))
            {
                balance = new AccountBalance
                {
                    AccountId = accountId
                };
                _accountBalances.Add(accountId, balance);
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
            return Task.CompletedTask;
        }
        
        private AccountDomainEvent ConvertEventDataToAccountDomainEvent(ResolvedEvent resolvedEvent)
        {
            var metadataString = Encoding.UTF8.GetString(resolvedEvent.Event.Metadata);

            var metadata = JsonConvert.DeserializeObject<DomainMetadata>(metadataString, _jsonSerializerSettings);

            if (metadata?.Schema == AccountSchema.SchemaName)
            {
                var eventType = _readSchema.GetDomainEventType(resolvedEvent.Event.EventType);

                if (eventType == null)
                    return null;

                var eventString = Encoding.UTF8.GetString(resolvedEvent.Event.Data);
                var accountEvent = (AccountDomainEvent)JsonConvert.DeserializeObject(eventString, eventType, _jsonSerializerSettings);
                accountEvent.StreamId = metadata.StreamId;

                return accountEvent;
            }

            return null;
        }
    }
}