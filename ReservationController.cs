using Microsoft.AspNetCore.Mvc;
using Fryzjer.Data;
using Fryzjer.Models;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace Fryzjer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReservationController : ControllerBase
    {
        private readonly FryzjerContext _context;

        public ReservationController(FryzjerContext context)
        {
            _context = context;
        }

        // DTO dla rezerwacji
        public class CreateReservationRequest
        {
            public string date { get; set; }
            public string hour { get; set; }
            public ClientInfo client { get; set; }
        }

        public class ClientInfo
        {
            public string? login { get; set; } // Opcjonalny login klienta
            public string name { get; set; }  // Imię klienta
            public string surname { get; set; } // Nazwisko klienta
            public string phone { get; set; } // Telefon klienta
        }

        // POST: api/Reservation
        [HttpPost]
        public IActionResult CreateReservation([FromBody] CreateReservationRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest("Nieprawidłowe dane rezerwacji.");
                }

                // Walidacja daty i godziny
                if (!DateTime.TryParse(request.date, out DateTime reservationDate))
                {
                    return BadRequest("Nieprawidłowy format daty.");
                }

                if (!TimeSpan.TryParse(request.hour, out TimeSpan startTime))
                {
                    return BadRequest("Nieprawidłowy format godziny rozpoczęcia.");
                }

                // Pobieramy ID fryzjera z sesji
                int? hairdresserId = HttpContext.Session.GetInt32("HairdresserId");
                if (hairdresserId == null)
                {
                    return Unauthorized("Nie jesteś zalogowany jako fryzjer.");
                }

                // Znajdujemy klienta na podstawie loginu lub tworzymy nowego
                Client client = null;
                if (!string.IsNullOrEmpty(request.client.login))
                {
                    client = _context.Client.FirstOrDefault(c => c.Login == request.client.login);
                }

                // Tworzenie klienta, jeśli nie istnieje
                if (client == null)
                {
                    client = new Client
                    {
                        Name = request.client.name,
                        Surname = request.client.surname,
                        Phone = request.client.phone,
                        Gender = 'N' // Domyślna wartość płci
                    };

                    _context.Client.Add(client);
                    _context.SaveChanges(); // Zapis nowego klienta
                }

                // Tworzenie obiektu rezerwacji
                var reservation = new Reservation
                {
                    date = reservationDate,
                    time = startTime,
                    status = 'A', // Domyślnie zaakceptowana
                    ClientId = client.Id,
                    HairdresserId = hairdresserId.Value
                };

                _context.Reservation.Add(reservation);
                _context.SaveChanges();

                return Ok("Rezerwacja została utworzona.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Wystąpił błąd podczas tworzenia rezerwacji: " + ex.Message);
            }
        }

        // GET: api/Reservation
        [HttpGet]
        public IActionResult GetReservations()
        {
            var reservations = _context.Reservation
                .Select(r => new
                {
                    r.Id,
                    r.date,
                    r.time,
                    r.status,
                    ClientName = r.Client != null ? r.Client.Name + " " + r.Client.Surname : null,
                    HairdresserName = r.Hairdresser != null ? r.Hairdresser.Name + " " + r.Hairdresser.surname : null
                })
                .ToList();

            return Ok(reservations);
        }

        // DELETE: api/Reservation/{id}
        [HttpDelete("{id}")]
        public IActionResult DeleteReservation(int id)
        {
            var reservation = _context.Reservation.Find(id);
            if (reservation == null)
            {
                return NotFound("Rezerwacja nie została znaleziona.");
            }

            _context.Reservation.Remove(reservation);
            _context.SaveChanges();

            return Ok("Rezerwacja została usunięta.");
        }
    }
}
