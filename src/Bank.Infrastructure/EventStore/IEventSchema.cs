namespace Bank.Infrastructure.EventStore
{
    using System;
    using Domain;

    public interface IEventSchema
    {
        string Name { get; }

        Type GetDomainEventType(string eventType);

        string GetEventDefinition(IDomainEvent domainEvent);
    }
}