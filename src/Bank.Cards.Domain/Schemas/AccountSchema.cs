namespace Bank.Cards.Domain.Schemas
{
    using Account.Events;
    using Infrastructure.EventStore;

    public class AccountSchema : EventSchema<AccountDomainEvent>
    {
        public const string SchemaName = "Account";

        public override string Name => SchemaName;
    }
}