namespace Bank.Cards.Domain.Invoice
{
    using System;
    using System.Threading.Tasks;
    using Infrastructure.EventStore;

    public class InvoiceRepository : IInvoiceRepository
    {
        private readonly IEventStore _eventStore;

        public InvoiceRepository(IEventStore eventStore)
        {
            _eventStore = eventStore;
        }

        public async Task<Invoice> GetInvoiceById(Guid id)
        {
            var domainEvents = await _eventStore.GetEventsByStreamId(new InvoiceEventStreamId(id));
            
            if (domainEvents.Count == 0)
                return null;

            return new Invoice(domainEvents);
        }

        public async Task SaveInvoice(Invoice invoice)
        {
            var streamVersion = invoice.StreamVersion - invoice.UncommittedEvents.Count;

            await _eventStore.SaveEvents(new InvoiceEventStreamId(invoice.Id), streamVersion, invoice.UncommittedEvents);
        }
    }
}