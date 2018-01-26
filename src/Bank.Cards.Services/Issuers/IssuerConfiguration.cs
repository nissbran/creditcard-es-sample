namespace Bank.Cards.Services.Issuers
{
    using System.Collections.Generic;
    using Domain.Configuration;

    public static class IssuerConfiguration
    {
        public static readonly Dictionary<long, Issuer> Issuers = new Dictionary<long, Issuer>
        {
            {1, new Issuer {Id = 1, Name = "Issuer 1"}},
            {2, new Issuer {Id = 2, Name = "Issuer 2"}},
        };
    }
}