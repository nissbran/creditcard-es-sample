using Bank.Infrastructure.Domain;

namespace Bank.Cards.Domain.Account.Events
{
    [EventName("MonthlyInvoicePeriodEnded")]
    public class MonthlyInvoicePeriodEndedEvent : AccountDomainEvent
    {

    }
}
