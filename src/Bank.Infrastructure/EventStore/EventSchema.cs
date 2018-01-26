namespace Bank.Infrastructure.EventStore
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Domain;

    public abstract class EventSchema<TBaseEvent> : IEventSchema where TBaseEvent : IDomainEvent
    {
        private readonly Dictionary<string, Type> _definitionToType = new Dictionary<string, Type>();
        private readonly Dictionary<Type, string> _typeToDefinition = new Dictionary<Type, string>();

        public abstract string Name { get; }

        protected EventSchema()
        {
            var baseEvent = typeof(TBaseEvent);
            var types = baseEvent.GetTypeInfo().Assembly.GetTypes()
                .Where(p => baseEvent.IsAssignableFrom(p));

            foreach (var type in types)
            {
                if (type.GetTypeInfo().GetCustomAttribute(typeof(EventNameAttribute)) is EventNameAttribute eventName)
                {
                    _definitionToType.Add(eventName.Name, type);
                    _typeToDefinition.Add(type, eventName.Name);
                }
            }
        }

        public Type GetDomainEventType(string eventType)
        {
            var eventDefinition = new EventDefinition(eventType);

            if (_definitionToType.TryGetValue(eventType, out var domainEvent))
                return domainEvent;

            throw new NotImplementedException();
        }

        public string GetEventDefinition(IDomainEvent domainEvent)
        {
            if (_typeToDefinition.TryGetValue(domainEvent.GetType(), out var eventDefinition))
                return eventDefinition;

            throw new NotImplementedException();
        }
    }
}