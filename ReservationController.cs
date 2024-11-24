using Microsoft.AspNetCore.Mvc;
using Fryzjer.Data;
using Fryzjer.Models;
using System.Linq;
using Microsoft.AspNetCore.Http;
using System;

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
            public string endHour { get; set; }
            public ClientInfo client { get; set; }
            public int serviceId { get; set; }
        }

        public class ClientInfo
        {
            public string? login { get; set; } // Opcjonalny login klienta
            public string name { get; set; }   // Imię klienta
            public string surname { get; set; } // Nazwisko klienta
            public string phone { get; set; }   // Telefon klienta
            public char gender { get; set; }    // Płeć klienta ('M', 'K', 'N')
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

                // Walidacja daty i godziny rozpoczęcia
                if (!DateTime.TryParse(request.date, out DateTime reservationDate))
                {
                    return BadRequest("Nieprawidłowy format daty.");
                }

                if (!TimeSpan.TryParse(request.hour, out TimeSpan startTime))
                {
                    return BadRequest("Nieprawidłowy format godziny rozpoczęcia.");
                }

                // Walidacja godziny zakończenia
                if (!TimeSpan.TryParse(request.endHour, out TimeSpan endTime))
                {
                    return BadRequest("Nieprawidłowy format godziny zakończenia.");
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

                // Sprawdzenie, czy klient już istnieje na podstawie imienia, nazwiska i telefonu
                if (client == null)
                {
                    client = _context.Client.FirstOrDefault(c =>
                        c.Name == request.client.name &&
                        c.Surname == request.client.surname &&
                        c.Phone == request.client.phone);
                }

                // Tworzenie klienta, jeśli nie istnieje
                if (client == null)
                {
                    client = new Client
                    {
                        Name = request.client.name,
                        Surname = request.client.surname,
                        Phone = request.client.phone,
                        Gender = request.client.gender
                    };

                    _context.Client.Add(client);
                    _context.SaveChanges(); // Zapis nowego klienta
                }

                // Tworzenie rezerwacji w ramach podanego zakresu czasowego (co 15 minut)
                var currentTime = startTime;
                while (currentTime < endTime)
                {
                    var reservation = new Reservation
                    {
                        date = reservationDate,
                        time = currentTime,
                        status = 'P', // Potwierdzona, ponieważ tworzy ją fryzjer
                        ClientId = client.Id,
                        HairdresserId = hairdresserId.Value,
                        ServiceId = request.serviceId
                    };

                    _context.Reservation.Add(reservation);
                    currentTime = currentTime.Add(new TimeSpan(0, 15, 0)); // Skok co 15 minut
                }

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
                .ToList() // Pobierz wszystkie dane z bazy danych
                .OrderBy(r => r.date) // Sortowanie po dacie
                .ThenBy(r => r.time)  // Sortowanie po czasie (już w pamięci)
                .Select(r => new
                {
                    r.Id,
                    r.date,
                    StartTime = r.time,
                    r.status,
                    ClientName = r.Client != null ? r.Client.Name + " " + r.Client.Surname : null,
                    ClientPhone = r.Client != null ? r.Client.Phone : null,
                    HairdresserName = r.Hairdresser != null ? r.Hairdresser.Name + " " + r.Hairdresser.Surname : null,
                    ServiceName = r.Service != null ? r.Service.Name : null
                })
                .ToList();

            return Ok(reservations);
        }

        // DELETE: api/Reservation/{id}
        [HttpDelete("{id}")]
        public IActionResult DeleteReservation(int id)
        {
            try
            {
                // Pobieramy ID fryzjera z sesji
                int? hairdresserId = HttpContext.Session.GetInt32("HairdresserId");
                if (hairdresserId == null)
                {
                    return Unauthorized("Nie jesteś zalogowany jako fryzjer.");
                }

                // Pobranie wszystkich powiązanych rezerwacji z danym identyfikatorem
                var reservations = _context.Reservation
                    .Where(r => r.Id == id && r.HairdresserId == hairdresserId.Value)
                    .ToList();

                if (reservations == null || reservations.Count == 0)
                {
                    return NotFound("Rezerwacja nie została znaleziona.");
                }

                // Usunięcie wszystkich powiązanych rezerwacji
                _context.Reservation.RemoveRange(reservations);
                _context.SaveChanges();

                return Ok("Rezerwacja została usunięta.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Wystąpił błąd podczas usuwania rezerwacji: " + ex.Message);
            }
        }
    }
}
