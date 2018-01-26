namespace Bank.Cards.Domain.Card
{
    using System.Threading.Tasks;

    public interface ICreditCardDomainRepository
    {
        Task<CreditCard> GetCardByHashedPan(string hashedPan);

        Task SaveCard(CreditCard card);
    }
}