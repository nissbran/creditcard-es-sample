namespace Bank.Infrastructure.Domain
{
    public struct EventDefinition
    {
        public EventDefinition(string eventName)
        {
            EventName = eventName;
        }

        public string EventName { get; }
    }
}