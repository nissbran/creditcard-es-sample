namespace Bank.Cards.Processes.InvoiceProcess.Actions
{
    using System;
    using System.Threading.Tasks;
    using Automatonymous;
    using Cards.Domain.Account.Events;
    using Cards.Domain.Invoice;
    using Cards.Domain.Invoice.Events;
    using Domain;
    using GreenPipes;

    public class AddDebitTransactionToInvoice : Activity<MonthlyInvoiceState, AccountDebitedEvent>
    {
        private readonly IInvoiceRepository _invoiceRepository;

        public AddDebitTransactionToInvoice(IInvoiceRepository invoiceRepository)
        {
            _invoiceRepository = invoiceRepository;
        }

        public void Probe(ProbeContext context)
        {
        }

        public void Accept(StateMachineVisitor visitor)
        {
        }

        public Task Execute(BehaviorContext<MonthlyInvoiceState, AccountDebitedEvent> context, Behavior<MonthlyInvoiceState, AccountDebitedEvent> next)
        {
            context.Instance.TotalAmountToPay += context.Data.Amount;

            var invoice = _invoiceRepository.GetInvoiceById(context.Instance.InvoiceId).Result;

            invoice.AddEvent(new InvoiceRowAddedEvent
            {
                Amount = context.Data.Amount
            });

            _invoiceRepository.SaveInvoice(invoice).Wait();

            return next.Execute(context);
        }

        public Task Faulted<TException>(BehaviorExceptionContext<MonthlyInvoiceState, AccountDebitedEvent, TException> context, Behavior<MonthlyInvoiceState, AccountDebitedEvent> next) where TException : Exception
        {
            return next.Faulted(context);
        }
    }
}
