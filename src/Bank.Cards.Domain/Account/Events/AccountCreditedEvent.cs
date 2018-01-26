﻿namespace Bank.Cards.Domain.Account.Events
{
    using Infrastructure.Domain;

    [EventName("AccountCredited")]
    public class AccountCreditedEvent : AccountDomainEvent
    {
        public decimal Amount { get; set; }

        public string Reference { get; set; }
    }
}