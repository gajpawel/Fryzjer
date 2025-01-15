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
                if (request == null)
                {
                    return BadRequest("Dane rezerwacji są wymagane.");
                }

                // Walidacja daty
                if (request.Date < DateTime.Now.Date)
                {
                    return BadRequest("Data rezerwacji musi być większa niż obecna.");
                }

                // Konwersja minut na TimeSpan (początek rezerwacji)
                TimeSpan startTime = TimeSpan.FromMinutes(request.Time);

                // Czas trwania usługi (przekazywany z formularza jako liczba minut)
                int serviceDuration = request.ServiceDuration; // Czas w minutach z formularza

              

                // Oblicz liczbę rezerwacji na podstawie czasu trwania usługi
                int numberOfReservations = (int)Math.Ceiling(serviceDuration / 15.0);

                for (int i = 0; i < numberOfReservations; i++)
                {
                    var reservation = new Reservation
                    {
                        date = request.Date,
                        time = startTime.Add(TimeSpan.FromMinutes(i * 15)), // Każda rezerwacja przesunięta o 15 minut
                        status = request.Status,
                        ClientId = request.ClientId,
                        HairdresserId = request.HairdresserId ?? 0,
                        ServiceId = request.ServiceId
                    };

                    _context.Reservation.Add(reservation);
                }

                _context.SaveChanges();
                return Ok("Rezerwacje zostały pomyślnie utworzone.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd podczas tworzenia rezerwacji.");
                return StatusCode(500, $"Wystąpił błąd podczas tworzenia rezerwacji: {ex.Message}");
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