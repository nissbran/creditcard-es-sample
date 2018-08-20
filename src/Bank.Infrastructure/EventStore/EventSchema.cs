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
        private readonly Dictionary<Type, EventDefinition> _typeToDefinition = new Dictionary<Type, EventDefinition>();

        public abstract string Name { get; }

        protected EventSchema()
        {
            var baseEvent = typeof(TBaseEvent);
            var types = baseEvent.GetTypeInfo().Assembly.GetTypes()
                .Where(p => baseEvent.IsAssignableFrom(p));

            foreach (var type in types)
            {
                if (type.GetTypeInfo().GetCustomAttribute(typeof(EventTypeAttribute)) is EventTypeAttribute eventType)
                {
                    _definitionToType.Add(new EventDefinition(eventType.Name, eventType.Version), type);
                    _typeToDefinition.Add(type, new EventDefinition(eventType.Name, eventType.Version));
                }
            }
        }

        public Type GetDomainEventType(string eventType)
        {
            if (_definitionToType.TryGetValue(eventType, out var domainEvent))
                return domainEvent;

            return null;
        }

        public bool EventTypeExistInSchema(string eventType)
        {
            return _definitionToType.ContainsKey(eventType);
        }

        public EventDefinition GetEventDefinition(IDomainEvent domainEvent)
        {
            if (_typeToDefinition.TryGetValue(domainEvent.GetType(), out var eventDefinition))
                return eventDefinition;

            throw new NotImplementedException();
        }
    }
}