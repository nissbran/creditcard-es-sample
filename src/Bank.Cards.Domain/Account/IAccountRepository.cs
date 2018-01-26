namespace Bank.Cards.Domain.Account
{
    using System;
    using System.Threading.Tasks;

    public interface IAccountRepository
    {
        Task<Account> GetAccountById(Guid cardId);

        Task SaveAccount(Account account);
    }
}