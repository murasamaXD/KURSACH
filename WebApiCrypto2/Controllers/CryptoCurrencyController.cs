using Microsoft.AspNetCore.Mvc;
using WebApiCrypto2.Clients;
using WebApiCrypto2.Models;
using System.Diagnostics;

namespace WebApiCrypto2.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CryptoCurrencyController : ControllerBase
    {
        private readonly ILogger<CryptoCurrencyController> _logger;

        public CryptoCurrencyController(ILogger<CryptoCurrencyController> logger)
        {
            _logger = logger;
        }

        [HttpGet("price", Name = "GetCryptoPrice")]
        public async Task<ActionResult<string>> GetCurrency(string currency)
        {
            try
            {
                CryptoClient client = new CryptoClient();
                var cryptoCurrencyData = await client.GetCryptoCurrencyByID(currency);

                if (cryptoCurrencyData != null && cryptoCurrencyData.ContainsKey(currency))
                {
                    var database = new Database();
                    await database.InsertCryptoCurrencyAsync(new CryptoCurrency { cryptocurrencies = cryptoCurrencyData });

                    return cryptoCurrencyData[currency].usd.ToString(System.Globalization.CultureInfo.InvariantCulture);
                }
                return NotFound("Currency not found");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching cryptocurrency price.");
                Debug.WriteLine($"Controller error: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
