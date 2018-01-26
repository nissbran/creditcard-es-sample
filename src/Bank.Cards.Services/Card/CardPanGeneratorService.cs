namespace Bank.Cards.Services.Card
{
    using System;

    public class CardPanGeneratorService
    {
        private const string Prefix = "521934";

        public string GeneratePan()
        {
            return Prefix + new Random().Next(10, 99) + new Random().Next(1000, 9999) + new Random().Next(1000, 9999);
        }
    }
}