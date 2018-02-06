namespace Bank.Cards.Domain.Account
{
    using System;
    using System.Collections.Generic;
    using Events;
    using Infrastructure.Domain;
    using State;

    public class Account
    {
        public string Id { get; private set; }

        public long StreamVersion { get; set; }

        public List<IDomainEvent> UncommittedEvents { get; } = new List<IDomainEvent>();
        
        public AccountState State { get; } = new AccountState();

        public Account(Guid id)
        {
            Id = id.ToString();
            AddEvent(new AccountCreatedEvent { AccountNumber = "Test" });
        }

        public Account(IEnumerable<IDomainEvent> historicEvents)
        {
            foreach (var historicEvent in historicEvents)
            {
                ApplyEvent((AccountDomainEvent)historicEvent);
                StreamVersion++;
            }
        }

        public void AddEvent(AccountDomainEvent domainEvent)
        {
            domainEvent.StreamId = Id;
            ApplyEvent(domainEvent);
            UncommittedEvents.Add(domainEvent);
            StreamVersion++;
        }

        private void ApplyEvent(AccountDomainEvent domainEvent)
        {
            Id = domainEvent.StreamId;

            switch (domainEvent)
            {
                case AccountCreatedEvent createdEvent:
                    State.AccountNumber = createdEvent.AccountNumber;
                    break;
                case AccountSnapShot snapshot:
                    State.Balance = snapshot.Balance;
                    State.AccountNumber = snapshot.AccountNumber;
                    StreamVersion = snapshot.SnapshotStreamVersion - 1;
                    break;
                case AccountDebitedEvent accountDebitedEvent:
                    State.Balance -= accountDebitedEvent.Amount;
                    break;
                case AccountDebitedEvent2 accountDebitedEvent2:
                    State.Balance -= (accountDebitedEvent2.AmountExcl + accountDebitedEvent2.VatAmount);
                    break;
                case AccountCreditedEvent accountCreditedEvent:
                    State.Balance += accountCreditedEvent.Amount;
                    break;
                case IssuerInformationSetEvent issuerInformationSetEvent:
                    State.IssuerId = issuerInformationSetEvent.IssuerId;
                    break;
            }
        }
    }
}