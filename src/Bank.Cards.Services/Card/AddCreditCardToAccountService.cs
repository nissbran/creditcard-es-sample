namespace Bank.Cards.Services.Card
{
    using System;
    using System.Threading.Tasks;
    using Domain.Account;
    using Domain.Card;
    using Domain.Card.Events;
    using Models;
    using Security;

    public class AddCreditCardToAccountService
    {
        private readonly IAccountRepository _accountRepository;
        private readonly ICreditCardDomainRepository _cardDomainRepository;

        private readonly CardPanGeneratorService _cardPanGeneratorService;
        private readonly PanEncryptService _panEncryptService;
        private readonly PanHashService _panHashService;

        public AddCreditCardToAccountService(IAccountRepository accountRepository, ICreditCardDomainRepository cardDomainRepository)
        {
            _accountRepository = accountRepository;
            _cardDomainRepository = cardDomainRepository;

            _cardPanGeneratorService = new CardPanGeneratorService();
            _panEncryptService = new PanEncryptService();
            _panHashService = new PanHashService();
        }

        public async Task<CreditCardInfo> AddCreditCardToAccount(Guid id, CreditCardInfo creditCardInfo)
        {
            var cardAccount = await _accountRepository.GetAccountById(id);

            if (cardAccount == null)
                return null;

            var newPan = _cardPanGeneratorService.GeneratePan();
            var hashedPan = _panHashService.HashPan(newPan);

            var newCard = new CreditCard(hashedPan);

            newCard.AddEvent(new CreditCardCreatedEvent
            {
                EncryptedPan = _panEncryptService.EncryptPan(newPan)
            });
            newCard.AddEvent(new CreditCardDetailsSetEvent
            {
                NameOnCard = creditCardInfo.NameOnCard
            });
            newCard.AddEvent(new CreditCardConnectedToAccountEvent
            {
                AccountId = Guid.Parse(cardAccount.Id)
            });

            await _cardDomainRepository.SaveCard(newCard);

            return new CreditCardInfo
            {
                NameOnCard = creditCardInfo.NameOnCard,
                Pan = newPan
            };
        }
    }
}