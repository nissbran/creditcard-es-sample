namespace Bank.Cards.Domain.Statistics
{
    using Infrastructure.EventStore;

    public class StatisticsAllCardsEventStreamId : EventStreamId
    {
        public override bool ResolveLinks { get; } = true;

        public override string StreamName => "$ce-Card";
    }
}