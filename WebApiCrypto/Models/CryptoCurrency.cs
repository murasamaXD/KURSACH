namespace WebApiCrypto.Models
{
    public class CryptoCurrency
    {
        public Dictionary<string, Crypto> crypto { get; set; }
    }

    public class Crypto
    {
        public decimal usd { get; set; }
    }
}