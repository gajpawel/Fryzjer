using Microsoft.AspNetCore.Mvc.RazorPages;
using Fryzjer.Models;
using Fryzjer.Data;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using System;

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
                .Where(r => r.date == date && r.HairdresserId == hairdresserId)
                .ToList() // Pobranie danych z bazy danych
                .OrderBy(r => r.time) // Sortowanie w pami�ci
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
                        ClientInfo = reservation.Client != null
                            ? $"{reservation.Client.Name} {reservation.Client.Surname}, Tel: {reservation.Client.Phone}, Us�uga: {reservation.Service?.Name}"
                            : "Brak danych klienta"
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
        public DateTime Date { get; set; } // Data dnia
        public List<TimeBlock> TimeBlocks { get; set; } = new List<TimeBlock>(); // Lista blok�w czasowych
    }

    // Klasa do reprezentowania bloku czasowego
    public class TimeBlock
    {
        public TimeSpan StartTime { get; set; } // Godzina rozpocz�cia
        public TimeSpan EndTime { get; set; } // Godzina zako�czenia
        public bool IsReserved { get; set; } // Czy blok jest zarezerwowany
        public string TimeRange => $"{StartTime:hh\\:mm} - {EndTime:hh\\:mm}"; // Zakres czasowy jako string
        public string? ClientInfo { get; set; } // Informacje o kliencie (je�li godzina jest zarezerwowana)
        public int? ReservationId { get; set; } // Identyfikator rezerwacji (je�li istnieje)
    }
}
