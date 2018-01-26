namespace Bank.Cards.Domain.Card.State
{
    using System;

    public class CardState
    {
        public bool CardEnabled { get; internal set; }

        public string NameOnCard { get; internal set; }

        public Guid AccountId { get; internal set; }
    }
}