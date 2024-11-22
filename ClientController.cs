using Microsoft.AspNetCore.Mvc;
using Fryzjer.Data;
using System.Linq;

namespace Fryzjer.Controllers
{
    [Route("api/client")]
    [ApiController]
    public class ClientController : ControllerBase
    {
        private readonly FryzjerContext _context;

        public ClientController(FryzjerContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult GetClientByLogin(string login)
        {
            var client = _context.Client.FirstOrDefault(c => c.Login == login);
            if (client == null)
                return NotFound("Klient nie został znaleziony.");

            return Ok(new
            {
                client.Name,
                client.Surname,
                client.Phone
            });
        }
    }
}
