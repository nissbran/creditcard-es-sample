using Bank.Infrastructure.Domain;

namespace Bank.Cards.Domain.Account.Events
{
    [EventType("MonthlyInvoicePeriodEnded")]
    public class MonthlyInvoicePeriodEndedEvent : AccountDomainEvent
    {

    }
}
