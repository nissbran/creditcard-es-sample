namespace Bank.Cards.Domain.Invoice
{
    using System;
    using System.Collections.Generic;
    using Events;
    using Infrastructure.Domain;
    using State;

    public class Invoice
    {
        public Guid Id { get; private set; }

        public long StreamVersion { get; set; }

        public List<IDomainEvent> UncommittedEvents { get; } = new List<IDomainEvent>();

        public InvoiceState State { get; } = new InvoiceState();

        public Invoice(Guid invoiceId, Guid accountId)
        {
            AddEvent(new InvoiceCreatedEvent
            {
                InvoiceId = invoiceId,
                AccountId = accountId
            });
        }

        public Invoice(IEnumerable<IDomainEvent> historicEvents)
        {
            foreach (var historicEvent in historicEvents)
            {
                ApplyEvent((InvoiceDomainEvent)historicEvent);
                StreamVersion++;
            }
        }

        public void AddEvent(InvoiceDomainEvent domainEvent)
        {
            ApplyEvent(domainEvent);
            UncommittedEvents.Add(domainEvent);
            StreamVersion++;
        }

        protected void ApplyEvent(InvoiceDomainEvent domainEvent)
        {
            switch (domainEvent)
            {
                case InvoiceCreatedEvent invoiceCreatedEvent:
                    Id = invoiceCreatedEvent.InvoiceId;
                    State.AccountId = invoiceCreatedEvent.AccountId;
                    break;
                case InvoiceSummaryAddedEvent InvoiceSummaryAddedEvent:
                    break;
            }
        }
    }
}