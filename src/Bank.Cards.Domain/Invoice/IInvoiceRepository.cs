namespace Bank.Cards.Domain.Invoice
{
    using System;
    using System.Threading.Tasks;

    public interface IInvoiceRepository
    {
        Task<Invoice> GetInvoiceById(Guid invoiceId);

        Task SaveInvoice(Invoice card);
    }
}