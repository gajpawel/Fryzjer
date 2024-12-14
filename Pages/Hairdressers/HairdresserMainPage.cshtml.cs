using Microsoft.AspNetCore.Mvc.RazorPages;
using Fryzjer.Models;
using Fryzjer.Data;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using System;
using Microsoft.EntityFrameworkCore;

namespace Fryzjer.Pages.Hairdressers
{
    public class HairdresserMainPageModel : PageModel
    {
        private readonly FryzjerContext _context;

        public HairdresserMainPageModel(FryzjerContext context)
        {
            _context = context;
        }

        // Harmonogram na bie��cy tydzie�
        public List<DailySchedule> WeeklySchedule1 { get; set; } = new List<DailySchedule>();

        // Harmonogram na kolejny tydzie�
        public List<DailySchedule> WeeklySchedule2 { get; set; } = new List<DailySchedule>();

        // Numer wy�wietlanego tygodnia (0 = bie��cy, -1 = poprzedni, 1 = nast�pny)
        public int CurrentWeek { get; set; } = 0;

        // Lista us�ug do wy�wietlenia w widoku
        public List<Service> Services { get; set; } = new List<Service>();

        public void OnGet(int week = 0)
        {
            // Pobieramy ID zalogowanego fryzjera z sesji
            int? hairdresserId = HttpContext.Session.GetInt32("HairdresserId");
            if (hairdresserId == null)
            {
                // Je�li fryzjer nie jest zalogowany, przekierowujemy na stron� logowania
                Response.Redirect("/Login");
                return;
            }

            CurrentWeek = week;

            // Pobieramy dost�pne us�ugi z bazy danych
            Services = _context.Service.ToList();

            // Obliczamy pierwszy dzie� wybranego tygodnia (poniedzia�ek)
            var startDate = DateTime.Now.Date.AddDays(7 * week - (int)DateTime.Now.DayOfWeek + 1);

            // Przygotowujemy harmonogramy dla dw�ch tygodni (poniedzia�ek-pi�tek)
            WeeklySchedule1 = GenerateSchedule(startDate, hairdresserId.Value);
            WeeklySchedule2 = GenerateSchedule(startDate.AddDays(7), hairdresserId.Value);
        }

        private List<DailySchedule> GenerateSchedule(DateTime startDate, int hairdresserId)
        {
            var schedule = new List<DailySchedule>();

            for (int i = 0; i < 5; i++) // Tylko dni robocze (poniedzia�ek�pi�tek)
            {
                var date = startDate.AddDays(i);
                var timeBlocks = new List<TimeBlock>();

                if (date >= DateTime.Now.Date) // Generujemy godziny tylko dla przysz�ych dat
                {
                    timeBlocks = GenerateDailyTimeBlocks(date, hairdresserId);
                }

                schedule.Add(new DailySchedule
                {
                    Date = date,
                    TimeBlocks = timeBlocks
                });
            }

            return schedule;
        }

        private List<TimeBlock> GenerateDailyTimeBlocks(DateTime date, int hairdresserId)
        {
            var blocks = new List<TimeBlock>();
            var startTime = new TimeSpan(8, 0, 0); // Start o 08:00
            var endTime = new TimeSpan(18, 0, 0); // Koniec o 18:00

            // Pobieramy istniej�ce rezerwacje na dany dzie� dla danego fryzjera
            var reservations = _context.Reservation
                .Include(r => r.Client)
                .Include(r => r.Service)
                .Where(r => r.date == date && r.HairdresserId == hairdresserId)
                .ToList()                  // Najpierw pobieramy dane
                .OrderBy(r => r.time)      // Potem sortujemy w pami�ci
                .ToList();

            TimeBlock currentBlock = null;

            foreach (var reservation in reservations)
            {
                if (currentBlock == null || reservation.time != currentBlock.EndTime)
                {
                    if (currentBlock != null)
                    {
                        blocks.Add(currentBlock);
                    }

                    currentBlock = new TimeBlock
                    {
                        StartTime = reservation.time,
                        EndTime = reservation.time.Add(new TimeSpan(0, 15, 0)),
                        IsReserved = true,
                        ReservationId = reservation.Id,
                        ClientInfo = $"{reservation.Client?.Name} {reservation.Client?.Surname}\nTel: {reservation.Client?.Phone}",
                        ServiceName = reservation.Service?.Name ?? "Brak us�ugi"
                    };
                }
                else
                {
                    currentBlock.EndTime = currentBlock.EndTime.Add(new TimeSpan(0, 15, 0));
                }
            }

            if (currentBlock != null)
            {
                blocks.Add(currentBlock);
            }

            return blocks;
        }
    }

    // Klasa do przechowywania harmonogramu jednego dnia
    public class DailySchedule
    {
        public DateTime Date { get; set; }
        public List<TimeBlock> TimeBlocks { get; set; } = new List<TimeBlock>();
    }

    // Klasa do reprezentowania bloku czasowego
    public class TimeBlock
    {
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public bool IsReserved { get; set; }
        public string TimeRange => $"{StartTime:hh\\:mm} - {EndTime:hh\\:mm}";
        public string? ClientInfo { get; set; }
        public int? ReservationId { get; set; }
        public string? ServiceName { get; set; }
    }
}