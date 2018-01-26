namespace Bank.Infrastructure.EventStore
{
    public abstract class EventStreamId
    {
        public abstract string StreamName { get; }

        public virtual bool ResolveLinks { get; } = false;

        public override string ToString()
        {
            return StreamName;
        }
    }
}