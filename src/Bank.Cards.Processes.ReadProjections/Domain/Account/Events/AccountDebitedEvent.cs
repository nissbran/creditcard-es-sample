﻿namespace Bank.Cards.Processes.ReadProjections.Domain.Account.Events
{
    using Domain.Events;
    using Infrastructure.Domain;

    [EventName("AccountDebited")]
    public class AccountDebitedEvent : AccountDomainEvent
    {
        public decimal Amount { get; set; }

        public string Reference { get; set; }

        public string CreatedBy { get; set; }
    }
}