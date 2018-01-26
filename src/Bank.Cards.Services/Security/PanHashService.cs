namespace Bank.Cards.Services.Security
{
    using System.Security.Cryptography;
    using System.Text;

    public class PanHashService
    {
        private const string SecretSalt = "ItsASecret";

        public string HashPan(string pan)
        {
            using (SHA256 shaM = new SHA256Managed())
            {
                var data = Encoding.UTF8.GetBytes(pan + SecretSalt);
                var hash = shaM.ComputeHash(data);

                var hashedInputStringBuilder = new StringBuilder(64);
                foreach (var b in hash)
                    hashedInputStringBuilder.Append(b.ToString("X2"));

                return hashedInputStringBuilder.ToString();
            }
        }
    }
}