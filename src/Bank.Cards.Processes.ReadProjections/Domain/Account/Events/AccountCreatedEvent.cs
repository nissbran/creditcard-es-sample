﻿namespace Bank.Cards.Processes.ReadProjections.Domain.Account.Events
{
    using Infrastructure.Domain;

    [EventType("AccountCreated")]
    public class AccountCreatedEvent : AccountDomainEvent
    {
        public string CurrencyIso { get; set; }

        public string AccountNumber { get; set; }
    }
}