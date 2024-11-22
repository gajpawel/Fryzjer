using Microsoft.AspNetCore.Mvc.RazorPages;
using Fryzjer.Models;
using Fryzjer.Data;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace Fryzjer.Pages.Hairdressers
{
    public class HairdresserMainPageModel : PageModel
    {
        private readonly FryzjerContext _context;

        public HairdresserMainPageModel(FryzjerContext context)
        {
            _context = context;
        }

        // Harmonogram na bie¿¹cy tydzieñ
        public List<DailySchedule> WeeklySchedule1 { get; set; } = new List<DailySchedule>();

        // Harmonogram na kolejny tydzieñ
        public List<DailySchedule> WeeklySchedule2 { get; set; } = new List<DailySchedule>();

        // Numer wyœwietlanego tygodnia (0 = bie¿¹cy, -1 = poprzedni, 1 = nastêpny)
        public int CurrentWeek { get; set; } = 0;

        // Lista us³ug do wyœwietlenia w widoku
        public List<Service> Services { get; set; } = new List<Service>();

        public void OnGet(int week = 0)
        {
            // Pobieramy ID zalogowanego fryzjera z sesji
            int? hairdresserId = HttpContext.Session.GetInt32("HairdresserId");
            if (hairdresserId == null)
            {
                // Jeœli fryzjer nie jest zalogowany, przekierowujemy na stronê logowania
                Response.Redirect("/Login");
                return;
            }

            CurrentWeek = week;

            // Pobieramy dostêpne us³ugi z bazy danych
            Services = _context.Service.ToList();

            // Obliczamy pierwszy dzieñ wybranego tygodnia (poniedzia³ek)
            var startDate = DateTime.Now.Date.AddDays(7 * week - (int)DateTime.Now.DayOfWeek + 1);

            // Przygotowujemy harmonogramy dla dwóch tygodni (poniedzia³ek-pi¹tek)
            WeeklySchedule1 = GenerateSchedule(startDate, hairdresserId.Value);
            WeeklySchedule2 = GenerateSchedule(startDate.AddDays(7), hairdresserId.Value);
        }

        private List<DailySchedule> GenerateSchedule(DateTime startDate, int hairdresserId)
        {
            var schedule = new List<DailySchedule>();

            for (int i = 0; i < 5; i++) // Tylko dni robocze (poniedzia³ek–pi¹tek)
            {
                var date = startDate.AddDays(i);
                var dailyHours = new List<HourStatus>();

                if (date >= DateTime.Now.Date) // Generujemy godziny tylko dla przysz³ych dat
                {
                    dailyHours = GenerateDailyHours(date, hairdresserId);
                }

                schedule.Add(new DailySchedule
                {
                    Date = date,
                    AvailableHours = dailyHours
                });
            }

            return schedule;
        }

        private List<HourStatus> GenerateDailyHours(DateTime date, int hairdresserId)
        {
            var hours = new List<HourStatus>();
            var startTime = new TimeSpan(8, 0, 0); // Start o 08:00
            var endTime = new TimeSpan(18, 0, 0); // Koniec o 18:00

            // Pobieramy istniej¹ce rezerwacje na dany dzieñ dla danego fryzjera
            var reservations = _context.Reservation
                .Where(r => r.date == date && r.HairdresserId == hairdresserId)
                .ToList();

            while (startTime < endTime)
            {
                var isReserved = reservations.Any(r => r.time == startTime);
                hours.Add(new HourStatus
                {
                    Time = startTime.ToString(@"hh\:mm"),
                    IsReserved = isReserved
                });
                startTime = startTime.Add(new TimeSpan(0, 15, 0)); // Skok co 15 minut
            }

            return hours;
        }
    }

    // Klasa do przechowywania harmonogramu jednego dnia
    public class DailySchedule
    {
        public DateTime Date { get; set; } // Data dnia
        public List<HourStatus> AvailableHours { get; set; } = new List<HourStatus>(); // Lista godzin
    }

    // Klasa do reprezentowania pojedynczej godziny
    public class HourStatus
    {
        public string Time { get; set; } // Godzina w formacie hh:mm
        public bool IsReserved { get; set; } // Czy godzina jest zarezerwowana
    }
}
