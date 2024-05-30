using Microsoft.AspNetCore.Mvc;
using WebApiCrypto2.Clients;
using WebApiCrypto2.Models;

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
                    // Повертаємо значення у форматі рядка
                    return cryptoCurrencyData[currency].usd.ToString(System.Globalization.CultureInfo.InvariantCulture);
                }
                return NotFound("Currency not found");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching cryptocurrency price.");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
