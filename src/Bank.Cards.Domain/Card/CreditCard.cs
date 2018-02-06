namespace Bank.Cards.Domain.Card
{
    using System;
    using System.Collections.Generic;
    using Events;
    using Infrastructure.Domain;
    using State;

    public class CreditCard
    {
        public string Id { get; protected set; }

        public long StreamVersion { get; set; }

        public List<IDomainEvent> UncommittedEvents { get; } = new List<IDomainEvent>();

        public CardState State { get; } = new CardState();

        public CreditCard(string hashedPan)
        {
            Id = hashedPan;

            AddEvent(new CreditCardCreatedEvent());
        }
        
        public CreditCard(IEnumerable<IDomainEvent> historicEvents)
        {
            foreach (var historicEvent in historicEvents)
            {
                ApplyEvent((CreditCardDomainEvent)historicEvent);
                StreamVersion++;
            }
        }

        public void AddEvent(CreditCardDomainEvent domainEvent)
        {
            ApplyEvent(domainEvent);

            UncommittedEvents.Add(domainEvent);

            StreamVersion++;
        }

        private void ApplyEvent(CreditCardDomainEvent domainEvent)
        {
            switch (domainEvent)
            {
                case CreditCardCreatedEvent creditCardCreatedEvent:
                    break;
                case CreditCardDetailsSetEvent cardDetailsSetEvent:
                    State.NameOnCard = cardDetailsSetEvent.NameOnCard;
                    break;
                case CreditCardConnectedToAccountEvent cardConnectedToAccountEvent:
                    State.AccountId = cardConnectedToAccountEvent.AccountId;
                    break;
                default:
                    throw new NotImplementedException();
            }
        }
    }
}