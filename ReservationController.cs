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
    public class ReservationController : ControllerBase
    {
        private readonly FryzjerContext _context;

        public ReservationController(FryzjerContext context)
        {
            _context = context;
        }

        public class CreateReservationRequest
        {
            public string date { get; set; }
            public string hour { get; set; }
            public string endHour { get; set; }
            public ClientInfo? client { get; set; }
            public int serviceId { get; set; }
        }

        public class ClientInfo
        {
            public string? login { get; set; }
            public string? name { get; set; }
            public string? surname { get; set; }
            public string? phone { get; set; }
            public char gender { get; set; }
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

                // Sprawdzamy czy to jest rezerwacja urlopu
                var vacationService = _context.Service.FirstOrDefault(s => s.Name.ToLower() == "urlop");
                bool isVacation = request.serviceId == vacationService?.Id;

                Client client = null;

                if (isVacation)
                {
                    // Dla urlopu, pobieramy lub tworzymy klienta na podstawie danych fryzjera
                    var hairdresser = _context.Hairdresser.FirstOrDefault(h => h.Id == hairdresserId);
                    if (hairdresser == null)
                    {
                        return NotFound("Nie znaleziono danych fryzjera.");
                    }

                    client = _context.Client.FirstOrDefault(c => c.Login == hairdresser.login);
                    if (client == null)
                    {
                        client = new Client
                        {
                            Name = hairdresser.Name,
                            Surname = hairdresser.Surname,
                            Login = hairdresser.login,
                            Gender = 'N',
                            Phone = "nieznany"
                        };
                        _context.Client.Add(client);
                        _context.SaveChanges();
                    }
                }
                else
                {
                    // Dla normalnej rezerwacji wymagamy danych klienta
                    if (request.client == null || string.IsNullOrEmpty(request.client.name) ||
                        string.IsNullOrEmpty(request.client.surname) || string.IsNullOrEmpty(request.client.phone))
                    {
                        return BadRequest("Wymagane dane klienta nie zostały podane.");
                    }

                    // Szukamy klienta po numerze telefonu i nazwisku
                    client = _context.Client.FirstOrDefault(c =>
                        c.Phone == request.client.phone &&
                        c.Surname == request.client.surname);

                    // Tworzenie nowego klienta, jeśli nie istnieje
                    if (client == null)
                    {
                        client = new Client
                        {
                            Name = request.client.name,
                            Surname = request.client.surname,
                            Phone = request.client.phone,
                            Gender = request.client.gender,
                            Login = request.client.login
                        };
                        _context.Client.Add(client);
                        _context.SaveChanges();
                    }

                    // Dla zwykłych rezerwacji sprawdzamy czy termin jest dostępny
                    var existingReservationsInDay = _context.Reservation
                        .Where(r => r.date == reservationDate &&
                                  r.HairdresserId == hairdresserId &&
                                  r.status != 'A') // Ignorujemy anulowane rezerwacje
                        .ToList();

                    var hasOverlap = existingReservationsInDay.Any(r =>
                        (r.time >= startTime && r.time < endTime) ||
                        (r.time.Add(new TimeSpan(0, 15, 0)) > startTime && r.time < endTime));

                    if (hasOverlap)
                    {
                        return BadRequest("Wybrany termin jest już zajęty.");
                    }
                }


                // Tworzenie rezerwacji w ramach podanego zakresu czasowego (co 15 minut)
                var currentTime = startTime;
                while (currentTime < endTime)
                {
                    var reservation = new Reservation
                    {
                        date = reservationDate,
                        time = currentTime,
                        status = isVacation ? 'O' : 'P', // O - Oczekuje (dla urlopu), P - Potwierdzona (dla normalnej)
                        ClientId = client.Id,
                        HairdresserId = hairdresserId.Value,
                        ServiceId = request.serviceId
                    };

                    _context.Reservation.Add(reservation);
                    currentTime = currentTime.Add(new TimeSpan(0, 15, 0));
                }


                _context.SaveChanges();
                return Ok(isVacation ? "Wniosek o urlop został złożony." : "Rezerwacja została utworzona.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Wystąpił błąd podczas tworzenia {(request.serviceId == _context.Service.FirstOrDefault(s => s.Name.ToLower() == "urlop")?.Id ? "wniosku o urlop" : "rezerwacji")}: {ex.Message}");
            }
        }

        // DELETE: api/Reservation/{id}
        [HttpDelete("{id}")]
        public IActionResult DeleteReservation(int id)
        {
            try
            {
                int? hairdresserId = HttpContext.Session.GetInt32("HairdresserId");
                if (hairdresserId == null)
                {
                    return Unauthorized("Nie jesteś zalogowany jako fryzjer.");
                }

                // Znajdujemy główną rezerwację
                var mainReservation = _context.Reservation
                    .Include(r => r.Service)
                    .FirstOrDefault(r => r.Id == id && r.HairdresserId == hairdresserId.Value);

                if (mainReservation == null)
                {
                    return NotFound("Rezerwacja nie została znaleziona.");
                }

                // Znajdujemy wszystkie rezerwacje z tego samego bloku czasowego
                var blockReservations = _context.Reservation
                    .Where(r =>
                        r.date == mainReservation.date &&
                        r.ClientId == mainReservation.ClientId &&
                        r.HairdresserId == hairdresserId.Value &&
                        r.ServiceId == mainReservation.ServiceId)
                    .ToList();

                // Usuwamy wszystkie znalezione rezerwacje
                _context.Reservation.RemoveRange(blockReservations);
                _context.SaveChanges();

                return Ok("Rezerwacja została usunięta.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Wystąpił błąd podczas usuwania rezerwacji: " + ex.Message);
            }
        }

        // POST: api/Reservation/{id}/confirm
        [HttpPost("{id}/confirm")]
        public IActionResult ConfirmReservation(int id)
        {
            try
            {
                int? hairdresserId = HttpContext.Session.GetInt32("HairdresserId");
                if (hairdresserId == null)
                {
                    return Unauthorized("Nie jesteś zalogowany jako fryzjer.");
                }

                var mainReservation = _context.Reservation
                    .FirstOrDefault(r => r.Id == id && r.HairdresserId == hairdresserId.Value);

                if (mainReservation == null)
                {
                    return NotFound("Rezerwacja nie została znaleziona.");
                }

                // Znajdujemy wszystkie rezerwacje z tego samego bloku czasowego
                var blockReservations = _context.Reservation
                    .Where(r =>
                        r.date == mainReservation.date &&
                        r.ClientId == mainReservation.ClientId &&
                        r.HairdresserId == mainReservation.HairdresserId &&
                        r.ServiceId == mainReservation.ServiceId)
                    .ToList();

                foreach (var reservation in blockReservations)
                {
                    reservation.status = 'P'; // Potwierdzona
                }

                _context.SaveChanges();
                return Ok("Rezerwacja została potwierdzona.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Wystąpił błąd podczas potwierdzania rezerwacji: " + ex.Message);
            }
        }

        // POST: api/Reservation/{id}/reject
        [HttpPost("{id}/reject")]
        public IActionResult RejectReservation(int id)
        {
            try
            {
                int? hairdresserId = HttpContext.Session.GetInt32("HairdresserId");
                if (hairdresserId == null)
                {
                    return Unauthorized("Nie jesteś zalogowany jako fryzjer.");
                }

                var mainReservation = _context.Reservation
                    .FirstOrDefault(r => r.Id == id && r.HairdresserId == hairdresserId.Value);

                if (mainReservation == null)
                {
                    return NotFound("Rezerwacja nie została znaleziona.");
                }

                // Znajdujemy wszystkie rezerwacje z tego samego bloku czasowego
                var blockReservations = _context.Reservation
                    .Where(r =>
                        r.date == mainReservation.date &&
                        r.ClientId == mainReservation.ClientId &&
                        r.HairdresserId == mainReservation.HairdresserId &&
                        r.ServiceId == mainReservation.ServiceId)
                    .ToList();

                foreach (var reservation in blockReservations)
                {
                    reservation.status = 'A'; // Anulowana
                }

                _context.SaveChanges();
                return Ok("Rezerwacja została odrzucona.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Wystąpił błąd podczas odrzucania rezerwacji: " + ex.Message);
            }
        }
    }
}