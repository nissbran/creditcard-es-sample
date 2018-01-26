namespace Bank.Cards.Services.Account
{
    using System;
    using System.Threading.Tasks;
    using Domain.Account;
    using Domain.Account.Events;
    using Models;

    public class CreateCreditAccountService
    {
        private readonly IAccountRepository _accountRepository;

        public CreateCreditAccountService(IAccountRepository accountRepository)
        {
            _accountRepository = accountRepository;
        }

        public async Task<string> CreateCreditAccount(CreditAccountInfo creditAccountInfo)
        {
            var account = new Account(Guid.NewGuid());

            account.AddEvent(new IssuerInformationSetEvent
            {
                IssuerId = creditAccountInfo.IssuerId
            });
            account.AddEvent(new CreditLimitSetEvent
            {
                CreditLimit = creditAccountInfo.CreditLimit
            });

            await _accountRepository.SaveAccount(account);

            return account.Id;
        }
    }
}