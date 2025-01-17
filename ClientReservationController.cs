using Microsoft.AspNetCore.Mvc;
using Fryzjer.Data;
using Fryzjer.Models;
using System.Linq;
using Microsoft.AspNetCore.Http;
using System;
using Microsoft.EntityFrameworkCore;

namespace Fryzjer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ClientReservationController : ControllerBase
    {
        private readonly FryzjerContext _context;
        private readonly ILogger<ClientReservationController> _logger;

        public ClientReservationController(FryzjerContext context, ILogger<ClientReservationController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpPost]
        public IActionResult CreateReservation([FromBody] CreateReservationRequest request)
        {
            try
            {
                // Sprawdzamy, czy request nie jest null
                if (request == null)
                {
                    _logger.LogError("Request is null.");
                    return BadRequest("Dane rezerwacji są wymagane.");
                }

                _logger.LogInformation("Request received for creating a reservation.");

                // Walidacja daty
                if (request.Date < DateTime.Now.Date)
                {
                    return BadRequest("Data rezerwacji musi być większa niż obecna.");
                }

                // Sprawdzamy, czy HairdresserId nie jest null
                if (!request.HairdresserId.HasValue)
                {
                    _logger.LogError("HairdresserId is null.");
                    return BadRequest("Id fryzjera jest wymagane.");
                }

                // Czas trwania usługi
                if (request.ServiceDuration <= 0)
                {
                    _logger.LogError("Invalid service duration.");
                    return BadRequest("Czas trwania usługi musi być większy niż 0 minut.");
                }

                TimeSpan startTime = TimeSpan.FromMinutes(request.Time);
                TimeSpan duration = TimeSpan.FromMinutes(request.ServiceDuration);
                TimeSpan endTime = startTime.Add(duration);

                TimeSpan workDayEnd = new TimeSpan(18, 0, 0); // 18:00
                if (endTime > workDayEnd)
                {
                    _logger.LogWarning("Requested reservation exceeds workday hours.");
                    return BadRequest("Czas zakończenia rezerwacji wykracza poza okres pracy. Proszę wybrać wcześniejszy termin.");
                }

                // Sprawdzanie, czy istnieją nakładające się rezerwacje
                var overlappingReservations = _context.Reservation
                    .Where(r => r.HairdresserId == request.HairdresserId && r.date == request.Date)
                    .AsEnumerable()
                    .Where(r => (r.time >= startTime && r.time < endTime))
                    .ToList();

                if (overlappingReservations.Any())
                {
                    _logger.LogWarning("Overlapping reservations detected.");
                    return BadRequest("Wybrany czas rezerwacji nakłada się na istniejącą rezerwację.");
                }

                int numberOfReservations = (int)Math.Ceiling(request.ServiceDuration / 15.0);
                _logger.LogInformation("Creating reservations.");

                for (int i = 0; i < numberOfReservations; i++)
                {
                    var reservation = new Reservation
                    {
                        date = request.Date,
                        time = startTime.Add(TimeSpan.FromMinutes(i * 15)),
                        status = request.Status,
                        ClientId = request.ClientId,
                        HairdresserId = request.HairdresserId.Value,
                        ServiceId = request.ServiceId
                    };

                    _context.Reservation.Add(reservation);
                }

                _context.SaveChanges();
                _logger.LogInformation("Reservations successfully created.");
                return Ok("Rezerwacje zostały pomyślnie utworzone.");
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while creating reservations.");
                return StatusCode(500, "Wystąpił błąd podczas tworzenia rezerwacji.");
            }
        }

        public class CreateReservationRequest
        {
            public DateTime Date { get; set; }
            public int Time { get; set; }  // Czas jako liczba minut od północy
            public char Status { get; set; }
            public int ClientId { get; set; }
            public int? HairdresserId { get; set; }
            public int ServiceId { get; set; }
            public int ServiceDuration { get; set; }
        }
    }
}
