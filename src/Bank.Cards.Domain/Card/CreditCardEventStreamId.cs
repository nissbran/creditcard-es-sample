namespace Bank.Cards.Domain.Card
{
    using Infrastructure.EventStore;

    public class CreditCardEventStreamId : EventStreamId
    {
        public string HashedPan { get; }

        public override string StreamName => $"Card-{HashedPan}";

        public CreditCardEventStreamId(string hashedPan)
        {
            HashedPan = hashedPan;
        }
    }
}