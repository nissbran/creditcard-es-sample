namespace Bank.Cards.Processes.InvoiceProcess.Actions
{
    using System;
    using System.Threading.Tasks;
    using Automatonymous;
    using Cards.Domain.Account.Events;
    using Domain;
    using GreenPipes;

    public class AddDebitTransactionToInvoice : Activity<MonthlyInvoiceState, AccountDebitedEvent>
    {
        public void Probe(ProbeContext context)
        {
        }

        public void Accept(StateMachineVisitor visitor)
        {
        }

        public Task Execute(BehaviorContext<MonthlyInvoiceState, AccountDebitedEvent> context, Behavior<MonthlyInvoiceState, AccountDebitedEvent> next)
        {
            context.Instance.TotalAmountToPay += context.Data.Amount;

            return next.Execute(context);
        }

        public Task Faulted<TException>(BehaviorExceptionContext<MonthlyInvoiceState, AccountDebitedEvent, TException> context, Behavior<MonthlyInvoiceState, AccountDebitedEvent> next) where TException : Exception
        {
            return next.Faulted(context);
        }
    }
}
