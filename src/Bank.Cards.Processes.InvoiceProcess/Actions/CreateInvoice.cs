namespace Bank.Cards.Processes.InvoiceProcess.Actions
{
    using System;
    using System.Threading.Tasks;
    using Automatonymous;
    using Cards.Domain.Account.Events;
    using Cards.Domain.Invoice;
    using Domain;
    using GreenPipes;

    public class CreateInvoice : Activity<MonthlyInvoiceState, AccountCreatedEvent>
    {
        private readonly IInvoiceRepository _invoiceRepository;

        public CreateInvoice(IInvoiceRepository invoiceRepository)
        {
            _invoiceRepository = invoiceRepository;
        }

        public void Probe(ProbeContext context)
        {
        }

        public void Accept(StateMachineVisitor visitor)
        {
        }

        public Task Execute(BehaviorContext<MonthlyInvoiceState, AccountCreatedEvent> context, Behavior<MonthlyInvoiceState, AccountCreatedEvent> next)
        {
            context.Instance.InvoiceId = Guid.NewGuid();
            context.Instance.AccountId = Guid.Parse(context.Data.StreamId);

            Console.WriteLine("Account created, starting on first invoice period");

            var newInvoice = new Invoice(context.Instance.InvoiceId, context.Instance.AccountId);

            _invoiceRepository.SaveInvoice(newInvoice).Wait();

            return next.Execute(context);
        }

        public Task Faulted<TException>(BehaviorExceptionContext<MonthlyInvoiceState, AccountCreatedEvent, TException> context, Behavior<MonthlyInvoiceState, AccountCreatedEvent> next) 
            where TException : Exception
        {
            return next.Faulted(context);
        }
    }
}
