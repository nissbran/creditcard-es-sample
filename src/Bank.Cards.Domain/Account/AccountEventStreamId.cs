namespace Bank.Cards.Domain.Account
{
    using System;
    using Infrastructure.EventStore;

    public class AccountEventStreamId : SnapshotEventStreamId
    {
        public string Id { get; }

        public override string StreamName => $"Account-{Id}";
        public override string SnapshotStreamName => $"Account-{Id}-Snapshot";

        public AccountEventStreamId(string id)
        {
            Id = id;
        }

        public AccountEventStreamId(Guid id) : this(id.ToString())
        {
        }
    }
}