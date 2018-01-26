namespace Bank.Cards.Services.Transactions
{
    using System.Threading.Tasks;
    using Domain.Account;
    using Domain.Account.Events;
    using Domain.Card;
    using Security;

    public class CreditCardPurchaseService
    {
        private readonly IAccountRepository _accountRepository;
        private readonly ICreditCardDomainRepository _cardDomainRepository;

        private readonly PanHashService _panHashService;

        public CreditCardPurchaseService(IAccountRepository accountRepository, ICreditCardDomainRepository cardDomainRepository)
        {
            _accountRepository = accountRepository;
            _cardDomainRepository = cardDomainRepository;

            _panHashService = new PanHashService();
        }

        public async Task<decimal?> CreditCardPurchase(string pan, decimal amount)
        {
            var hashedPan = _panHashService.HashPan(pan);
            var card = await _cardDomainRepository.GetCardByHashedPan(hashedPan);

            if (card == null)
                return null;

            var account = await _accountRepository.GetAccountById(card.State.AccountId);

            account.AddEvent(new AccountDebitedEvent
            {
                Amount = amount
            });

            await _accountRepository.SaveAccount(account);

            return account.State.Balance;
        }
    }
}