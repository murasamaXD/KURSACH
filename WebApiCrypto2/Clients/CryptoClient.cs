using Newtonsoft.Json;
using System;
using WebApiCrypto2.Models;

namespace WebApiCrypto2.Clients
{
    public class CryptoClient
    {
        private static string _address;
        private static string _apikeyRapid;
        private static string _apiHost;

        public CryptoClient()
        {
            _address = Constants.AddressGekko;
            _apikeyRapid = Constants.ApiKeyRapid;
            _apiHost = Constants.ApiHost;
        }

        public async Task<Dictionary<string, CurrencyData>> GetCryptoCurrencyByID(string id)
        {
            try
            {
                var client = new HttpClient();
                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri($"{_address}/api/v3/simple/price?ids={id}&vs_currencies=usd"),
                    Headers =
            {
                { "X-RapidAPI-Key", _apikeyRapid },
                { "X-RapidAPI-Host", _apiHost },
            },
                };
                using (var response = await client.SendAsync(request))
                {
                    response.EnsureSuccessStatusCode();
                    var body = await response.Content.ReadAsStringAsync();

                    var result = JsonConvert.DeserializeObject<Dictionary<string, CurrencyData>>(body);

                    return result;
                }
            }
            catch (HttpRequestException ex)
            {
                // Логування помилки
                Console.WriteLine($"Request error: {ex.Message}");
                throw;
            }
        }

    }
}
