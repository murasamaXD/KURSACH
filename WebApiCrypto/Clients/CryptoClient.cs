using Newtonsoft.Json;
using WebApiCrypto.Models;

namespace WebApiCrypto.Clients
{
    public class CryptoClient
    {
        private static string _address;
        private HttpClient _client;

        public CryptoClient()
        {
            _address = Constants.AddressGekko;
            _client = new HttpClient { BaseAddress = new Uri(_address) };
        }

        public async Task<CryptoCurrency> GetCryptoCurrencyByID(string id)
        {
            //var response = await _client.GetAsync($"/api/v3/simple/price?ids={id}&vs_currencies=usd");
            //response.EnsureSuccessStatusCode();
            //var content = await response.Content.ReadAsStringAsync();
            //var result = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, decimal>>>(content);
            //var cryptoData = new CryptoCurrency
            //{
            //    crypto = new Dictionary<string, Crypto>
            //    {
            //        [id] = new Crypto { usd = result[id]["usd"] }
            //    }
            //};
            //return cryptoData;
            var client = new HttpClient();
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri("https://coingecko.p.rapidapi.com/simple/price?vs_currencies=usd&ids=bitcoin"),
                Headers =
                {
                    { "X-RapidAPI-Key", "9c9e7b81c0msh290ab7a973d2150p155236jsnfab98b8bbe7f" },
                    { "X-RapidAPI-Host", "coingecko.p.rapidapi.com" },
                },
            };
            using (var response = await client.SendAsync(request))
            {
                response.EnsureSuccessStatusCode();
                var body = await response.Content.ReadAsStringAsync();
                Console.WriteLine(body);
            }
        }
    }
}