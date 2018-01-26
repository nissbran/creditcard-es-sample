namespace Bank.Cards.Processes.InvoiceProcess.Process
{
    using Actions;
    using Automatonymous;
    using Cards.Domain.Account.Events;
    using Cards.Domain.Invoice;
    using Domain;

    public class MonthlyInvoiceStateMachine : AutomatonymousStateMachine<MonthlyInvoiceState>
    {
        public MonthlyInvoiceStateMachine(IInvoiceRepository invoiceRepository)
        {
            var invoiceDomainRepository = invoiceRepository;

            InstanceState(x => x.CurrentState);

            During(Initial,
                    When(AccountCreated)
                        .Execute(context => new CreateInvoice(invoiceDomainRepository))
                        .TransitionTo(InProcess));

            During(InProcess,
                    When(AccountDebited)
                        .Execute(context => new AddDebitTransactionToInvoice()),
                    When(MonthlyInvoicePeriodEnded)
                        .Execute(context => new FinishMonthlyInvoice(invoiceDomainRepository)));
        }

        public State InProcess { get; set; }
        public State InvoiceCreated { get; set; }

        public Event<AccountCreatedEvent> AccountCreated { get; set; }
        public Event<AccountDebitedEvent> AccountDebited { get; set; }
        public Event<MonthlyInvoicePeriodEndedEvent> MonthlyInvoicePeriodEnded { get; set; }
    }
}
