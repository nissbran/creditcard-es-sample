namespace Bank.Cards.Domain.Statistics
{
    using Infrastructure.EventStore;

    public class StatisticsAllAccountEventStreamId : EventStreamId
    {
        public override bool ResolveLinks { get; } = true;

        public override string StreamName => "$ce-Account";
    }
}