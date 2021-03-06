﻿namespace Bank.Cards.Processes.ReadProjections.Domain.Account.Events
{
    using Infrastructure.Domain;

    [EventType("AccountDebited")]
    public class AccountDebitedEvent : AccountDomainEvent
    {
        public decimal Amount { get; set; }

        

        public decimal VatAmount { get; set; }

        public string Reference { get; set; }

        public string CreatedBy { get; set; }
    }
}