namespace Bank.Infrastructure.Domain
{
    public struct EventDefinition
    {
        public EventDefinition(string eventName) : this(eventName, 1)
        {
        }

        public EventDefinition(string eventName, int latestVersion)
        {
            EventName = eventName;
            LatestVersion = latestVersion;
        }

        public string EventName { get; }

        public int LatestVersion { get; }

        public static implicit operator string(EventDefinition definition)
        {
            return definition.EventName;
        }
    }
}