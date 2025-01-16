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

                _logger.LogInformation($"Request received: Date={request.Date}, Time={request.Time}, ServiceDuration={request.ServiceDuration}, HairdresserId={request.HairdresserId}");

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

                // Czas trwania usługi (przekazywany z formularza jako liczba minut)
                int serviceDuration = request.ServiceDuration;
                if (serviceDuration <= 0)
                {
                    _logger.LogError("ServiceDuration is invalid or zero.");
                    return BadRequest("Czas trwania usługi musi być większy niż 0 minut.");
                }

                // Konwersja minut na TimeSpan (początek rezerwacji)
                TimeSpan startTime = TimeSpan.FromMinutes(request.Time);
                _logger.LogInformation($"Start time: {startTime}");

                // Tworzymy obiekt TimeSpan z czasu trwania usługi
                TimeSpan duration = TimeSpan.FromMinutes(serviceDuration);
                _logger.LogInformation($"Service duration: {duration}");

                // Obliczamy czas zakończenia rezerwacji
                TimeSpan endTime = startTime.Add(duration);
                _logger.LogInformation($"End time: {endTime}");

                    // Sprawdzamy, czy w tym czasie istnieje już jakaś rezerwacja dla fryzjera
                    var overlappingReservations = _context.Reservation
                        .Where(r => r.HairdresserId == request.HairdresserId &&
                                    r.date == request.Date)
                        .AsEnumerable() // Wymusza przetłumaczenie na kod po stronie klienta
                        .Where(r => (r.time >= startTime && r.time < endTime))
                        .ToList();


                // Jeśli są rezerwacje nakładające się, logujemy szczegóły
                if (overlappingReservations.Any())
                {
                    // Logowanie szczegółów wszystkich nakładających się rezerwacji
                    foreach (var reservation in overlappingReservations)
                    {
                        // Wyświetlanie szczegółów w logach
                        _logger.LogWarning($"Overlapping reservation found: HairdresserId={reservation.HairdresserId}, " +
                                           $"Date={reservation.date}, Time={reservation.time}, " +
                                           $"EndTime={reservation.time.Add(TimeSpan.FromMinutes(serviceDuration))}");
                    }

                    // Logujemy również szczegóły obliczonych czasów
                    _logger.LogWarning($"Requested Start Time: {startTime}, Requested End Time: {endTime}, " +
                                       $"Service Duration: {serviceDuration} minutes.");

                    // Wysyłamy szczegóły w odpowiedzi BadRequest
                    return BadRequest(new
                    {
                        Message = "Wybrany czas jest już zarezerwowany. Proszę wybrać inny termin.",
                        RequestedStartTime = startTime,
                        RequestedEndTime = endTime,
                        ServiceDurationMinutes = serviceDuration,
                        OverlappingReservations = overlappingReservations.Select(r => new
                        {
                            r.HairdresserId,
                            r.date,
                            r.time,
                            EndTime = r.time.Add(TimeSpan.FromMinutes(serviceDuration))
                        })
                    });
                }

                // Oblicz liczbę rezerwacji na podstawie czasu trwania usługi
                int numberOfReservations = (int)Math.Ceiling(serviceDuration / 15.0);
                _logger.LogInformation($"Number of reservations to be created: {numberOfReservations}");

                for (int i = 0; i < numberOfReservations; i++)
                {
                    var reservation = new Reservation
                    {
                        date = request.Date,
                        time = startTime.Add(TimeSpan.FromMinutes(i * 15)), // Każda rezerwacja przesunięta o 15 minut
                        status = request.Status,
                        ClientId = request.ClientId,
                        HairdresserId = request.HairdresserId.Value, // Używamy .Value, bo HairdresserId jest typu int? (nullable)
                        ServiceId = request.ServiceId
                    };

                    _logger.LogInformation($"Adding reservation: {reservation.time}, HairdresserId={reservation.HairdresserId}, ClientId={reservation.ClientId}");

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