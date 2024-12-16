using Fryzjer.Data;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Fryzjer.Models;

namespace Fryzjer.Pages.AbstractFactory
{
    /// <summary>
    /// Klasa implementuj¹ca fabrykê harmonogramu dla fryzjera
    /// </summary>
    public class HairdresserScheduleFactoryModel : ScheduleFactoryModel
    {
        private readonly FryzjerContext _context;
        // Tabela harmonogramu z blokami czasowymi dla ka¿dego dnia
        public Dictionary<TimeSpan, Dictionary<DayOfWeek, TimeBlock>> ScheduleTable { get; set; }

        public HairdresserScheduleFactoryModel(FryzjerContext context)
        {
            _context = context;
            ScheduleTable = new Dictionary<TimeSpan, Dictionary<DayOfWeek, TimeBlock>>();
        }

        // Implementacja metody abstrakcyjnej z klasy bazowej
        public override void OnGet(int week = 0)
        {
            // Pobieranie ID zalogowanego fryzjera
            int? hairdresserId = HttpContext.Session.GetInt32("HairdresserId");
            if (hairdresserId == null)
            {
                Response.Redirect("/Login");
                return;
            }

            CurrentWeek = week;

            // Pobierz wszystkie us³ugi dla rozwijanej listy w modalu
            Services = _context.Service.ToList();

            // Oblicz pierwszy dzieñ tygodnia (poniedzia³ek)
            var startDate = DateTime.Now.Date.AddDays(7 * week - (int)DateTime.Now.DayOfWeek + 1);

            // Generowanie harmonogramu dla fryzjera
            ScheduleTable = GenerateSchedule(startDate, hairdresserId.Value);
        }

        /// <summary>
        /// Generuje harmonogram dla konkretnego fryzjera w okreœlonym tygodniu
        /// </summary>
        /// <param name="startDate">Data pocz¹tku tygodnia</param>
        /// <param name="hairdresserId">ID fryzjera</param>
        private Dictionary<TimeSpan, Dictionary<DayOfWeek, TimeBlock>> GenerateSchedule(DateTime startDate, int hairdresserId)
        {
            // Inicjalizacja s³ownika harmonogramu
            var schedule = new Dictionary<TimeSpan, Dictionary<DayOfWeek, TimeBlock>>();
            var startTime = new TimeSpan(8, 0, 0); // Start o 8:00
            var endTime = new TimeSpan(18, 0, 0); // Koniec o 18:00

            // Inicjalizuj puste bloki dla ka¿dego przedzia³u czasu
            for (var time = startTime; time < endTime; time = time.Add(new TimeSpan(0, 15, 0)))
            {
                schedule[time] = Enum.GetValues(typeof(DayOfWeek))
                    .Cast<DayOfWeek>()
                    .Where(d => d >= DayOfWeek.Monday && d <= DayOfWeek.Friday)
                    .ToDictionary(d => d, d => new TimeBlock
                    {
                        StartTime = time,
                        EndTime = time.Add(new TimeSpan(0, 15, 0)),
                        IsReserved = false,
                        BlockClass = "available",
                        Status = 'N' // N dla normalnego (niezarezerwowanego) bloku
                    });
            }

            // Pobierz us³ugê "urlop"
            var vacationService = _context.Service.FirstOrDefault(s => s.Name.ToLower() == "urlop");

            // Pobieranie istniej¹cych rezerwacji dla fryzjera w danym tygodniu
            var reservations = _context.Reservation
                .Include(r => r.Client)
                .Include(r => r.Service)
                .Where(r => r.date >= startDate &&
                           r.date < startDate.AddDays(7) &&
                           r.HairdresserId == hairdresserId)
                .OrderBy(r => r.date)
                .ThenBy(r => r.time)
                .ToList();

            // Mapowanie rezerwacji na odpowiednie pola w tabeli
            foreach (var reservation in reservations)
            {
                var day = reservation.date.DayOfWeek;
                var time = reservation.time;

                if (schedule.ContainsKey(time) && schedule[time].ContainsKey(day))
                {
                    var displayInfo = "";
                    var blockClass = "reserved";

                    if (reservation.ServiceId == vacationService?.Id)
                    {
                        // Wyœwietlanie statusu urlopu
                        var statusText = reservation.status switch
                        {
                            'O' => "Urlop (oczekuje)",
                            'P' => "Urlop (potwierdzony)",
                            'A' => "Urlop (anulowany)",
                            _ => "Urlop"
                        };
                        displayInfo = statusText;
                        blockClass = $"vacation-{reservation.status.ToString().ToLower()}";
                    }
                    else
                    {
                        // Standardowa rezerwacja
                        displayInfo = reservation.Client != null ?
                            $"{reservation.Client.Name} {reservation.Client.Surname}\nTel: {reservation.Client.Phone}" :
                            "Zarezerwowane";
                    }

                    schedule[time][day] = new TimeBlock
                    {
                        StartTime = time,
                        EndTime = time.Add(new TimeSpan(0, 15, 0)),
                        IsReserved = true,
                        ClientInfo = displayInfo,
                        ServiceName = reservation.Service?.Name,
                        ServiceId = reservation.ServiceId,
                        ReservationId = reservation.Id,
                        BlockClass = blockClass,
                        Status = reservation.status
                    };
                }
            }

            return schedule;
        }

        /// <summary>
        /// Obs³uguje ¿¹danie POST dla sk³adania wniosku o urlop
        /// </summary>
        public IActionResult OnPostVacationRequest(DateTime date, TimeSpan startTime, TimeSpan endTime)
        {
            // Sprawdzenie czy fryzjer jest zalogowany
            int? hairdresserId = HttpContext.Session.GetInt32("HairdresserId");
            if (hairdresserId == null)
            {
                return RedirectToPage("/Login");
            }

            try
            {
                // Pobierz us³ugê "urlop"
                var vacationService = _context.Service.FirstOrDefault(s => s.Name.ToLower() == "urlop");
                if (vacationService == null)
                {
                    throw new Exception("Nie znaleziono us³ugi typu urlop w systemie.");
                }

                // SprawdŸ czy nie ma ju¿ rezerwacji w tym czasie
                var existingReservations = _context.Reservation
                    .Any(r => r.date == date &&
                             r.HairdresserId == hairdresserId &&
                             ((r.time >= startTime && r.time < endTime) ||
                              (r.time.Add(new TimeSpan(0, 15, 0)) > startTime && r.time < endTime)));

                if (existingReservations)
                {
                    throw new Exception("Istniej¹ ju¿ rezerwacje w wybranym czasie.");
                }

                // Tworzenie bloków urlopowych (co 15 minut)
                var currentTime = startTime;
                while (currentTime < endTime)
                {
                    var reservation = new Reservation
                    {
                        date = date,
                        time = currentTime,
                        status = 'O', // Oczekuje na potwierdzenie
                        ClientId = 0,
                        HairdresserId = hairdresserId.Value,
                        ServiceId = vacationService.Id
                    };

                    _context.Reservation.Add(reservation);
                    currentTime = currentTime.Add(new TimeSpan(0, 15, 0));
                }

                _context.SaveChanges();
                return RedirectToPage();
            }
            catch (Exception ex)
            {
                // W przypadku b³êdu, przekazanie komunikatu do widoku
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToPage();
            }
        }
    }
}