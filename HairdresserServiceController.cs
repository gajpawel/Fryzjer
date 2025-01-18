using Microsoft.AspNetCore.Mvc;
using Fryzjer.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace Fryzjer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HairdresserServiceController : ControllerBase
    {
        private readonly FryzjerContext _context;

        public HairdresserServiceController(FryzjerContext context)
        {
            _context = context;
        }

        [HttpGet("services")]
        public IActionResult GetHairdresserServices()
        {
            var hairdresserId = HttpContext.Session.GetInt32("HairdresserId");
            if (hairdresserId == null)
            {
                return Unauthorized("Nie jesteś zalogowany jako fryzjer.");
            }

            // Pobierz unikalne usługi na podstawie historii rezerwacji
            var services = _context.Reservation
                .Where(r => r.HairdresserId == hairdresserId)
                .Select(r => r.Service)
                .Where(s => s.Name.ToLower() != "urlop")
                .Distinct()
                .Select(s => new
                {
                    s.Id,
                    s.Name,
                    s.Duration,
                    s.Price,
                    s.Color
                })
                .ToList();

            return Ok(services);
        }
    }
}