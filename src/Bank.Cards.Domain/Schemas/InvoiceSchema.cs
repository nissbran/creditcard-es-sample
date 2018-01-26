namespace Bank.Cards.Domain.Schemas
{
    using Card.Events;
    using Infrastructure.EventStore;
    using Invoice.Events;

    public class InvoiceSchema : EventSchema<InvoiceDomainEvent>
    {
        public const string SchemaName = "Invoice";

        public override string Name => SchemaName;
    }
}