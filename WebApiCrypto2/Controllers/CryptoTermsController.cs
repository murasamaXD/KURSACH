using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApiCrypto2.Models;

namespace WebApiCrypto2.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CryptoTermsController : ControllerBase
    {
        private readonly Database _database;

        public CryptoTermsController()
        {
            _database = new Database();
        }


        [HttpGet]
        public async Task<ActionResult<IEnumerable<CryptoTerm>>> GetTerms()
        {
            var terms = await _database.GetCryptoTermsAsync();
            return Ok(terms);
        }

        [HttpGet("name/{termName}")]
        public async Task<ActionResult<CryptoTerm>> GetTermByName(string termName)
        {
            var term = await _database.GetCryptoTermByNameAsync(termName);
            if (term == null)
            {
                return NotFound();
            }
            return Ok(term);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CryptoTerm>> GetTerm(int id)
        {
            var term = await _database.GetCryptoTermByIdAsync(id);
            if (term == null)
            {
                return NotFound();
            }
            return Ok(term);
        }

        [HttpPost]
        public async Task<ActionResult> CreateTerm(CryptoTerm term)
        {
            await _database.InsertCryptoTermAsync(term);
            return CreatedAtAction(nameof(GetTerm), new { id = term.Id }, term);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTerm(int id, CryptoTerm term)
        {
            if (id != term.Id)
            {
                return BadRequest();
            }

            await _database.UpdateCryptoTermAsync(term);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTerm(int id)
        {
            await _database.DeleteCryptoTermAsync(id);
            return NoContent();
        }
    }
}
