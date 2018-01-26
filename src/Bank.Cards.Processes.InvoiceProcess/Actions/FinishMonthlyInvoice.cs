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

    public class FinishMonthlyInvoice : Activity<MonthlyInvoiceState, MonthlyInvoicePeriodEndedEvent>
    {
        private readonly IInvoiceRepository _invoiceRepository;

        public FinishMonthlyInvoice(IInvoiceRepository invoiceRepository)
        {
            _invoiceRepository = invoiceRepository;
        }

        public void Accept(StateMachineVisitor visitor)
        {
        }

        public Task Execute(BehaviorContext<MonthlyInvoiceState, MonthlyInvoicePeriodEndedEvent> context, Behavior<MonthlyInvoiceState, MonthlyInvoicePeriodEndedEvent> next)
        {
            if (context.Instance.TotalAmountToPay > 0)
            {
                Console.WriteLine($"Account monthly invoice period is over, saving summary for TotalAmountToPay: {context.Instance.TotalAmountToPay}");

                var invoice = _invoiceRepository.GetInvoiceById(context.Instance.InvoiceId).Result;

                invoice.AddEvent(new InvoiceSummaryAddedEvent
                {
                    TotalAmountToPay = context.Instance.TotalAmountToPay
                });

                _invoiceRepository.SaveInvoice(invoice).Wait();
            }

            context.Instance.InvoiceId = Guid.NewGuid();
            context.Instance.TotalAmountToPay = 0;
            context.Instance.AccountId = Guid.Parse(context.Data.StreamId);

            Console.WriteLine("Account monthly invoice period is over, starting on a new invoice period");

            var newInvoice = new Invoice(context.Instance.InvoiceId, context.Instance.AccountId);

            _invoiceRepository.SaveInvoice(newInvoice).Wait();

            return next.Execute(context);
        }

        public Task Faulted<TException>(BehaviorExceptionContext<MonthlyInvoiceState, MonthlyInvoicePeriodEndedEvent, TException> context, Behavior<MonthlyInvoiceState, MonthlyInvoicePeriodEndedEvent> next) where TException : Exception
        {
            return next.Faulted(context);
        }

        public void Probe(ProbeContext context)
        {
        }
    }
}
