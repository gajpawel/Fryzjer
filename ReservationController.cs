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
            public string? login { get; set; }
            public string name { get; set; }
            public string surname { get; set; }
            public string phone { get; set; }
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

                // Szukamy klienta po numerze telefonu i nazwisku
                var client = _context.Client
                    .FirstOrDefault(c =>
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
                        Login = request.client.login // opcjonalne
                    };

                    _context.Client.Add(client);
                    _context.SaveChanges();
                }

                // Tworzenie rezerwacji w ramach podanego zakresu czasowego (co 15 minut)
                var currentTime = startTime;
                while (currentTime < endTime)
                {
                    var reservation = new Reservation
                    {
                        date = reservationDate,
                        time = currentTime,
                        status = 'P', // Potwierdzona
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
    }
}