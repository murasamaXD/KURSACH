namespace WebApiCrypto2.Models
{
    public class CryptoCurrency
    {
        public Dictionary<string, CurrencyData> cryptocurrencies { get; set; }
    }

    public class CurrencyData
    {
        public decimal usd { get; set; }
    }
}
