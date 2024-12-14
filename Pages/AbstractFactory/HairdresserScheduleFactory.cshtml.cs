using Fryzjer.Data;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Fryzjer.Pages.AbstractFactory
{
    public class HairdresserScheduleFactoryModel : ScheduleFactoryModel
    {
        private readonly FryzjerContext _context;

        // NOWA W£AŒCIWOŒÆ ScheduleTable
        public Dictionary<TimeSpan, Dictionary<DayOfWeek, TimeBlock>> ScheduleTable { get; set; }

        public HairdresserScheduleFactoryModel(FryzjerContext context)
        {
            _context = context;
            ScheduleTable = new Dictionary<TimeSpan, Dictionary<DayOfWeek, TimeBlock>>();
        }

        public override void GenerateSchedule(int week)
        {
            // Pobieranie ID zalogowanego fryzjera
            int? hairdresserId = HttpContext.Session.GetInt32("HairdresserId");
            if (hairdresserId == null)
            {
                Response.Redirect("/Login");
                return;
            }

            CurrentWeek = week;

            // Oblicz pierwszy dzieñ tygodnia (poniedzia³ek)
            var startDate = DateTime.Now.Date.AddDays(7 * week - (int)DateTime.Now.DayOfWeek + 1);

            // Generowanie harmonogramu dla fryzjera
            ScheduleTable = GenerateSchedule(startDate, hairdresserId.Value);
        }

        private Dictionary<TimeSpan, Dictionary<DayOfWeek, TimeBlock>> GenerateSchedule(DateTime startDate, int hairdresserId)
        {
            // Tworzenie pustej tabeli z czasami (co 15 minut) i dniami tygodnia (poniedzia³ek-pi¹tek)
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
                        IsReserved = false
                    });
            }

            // Pobieranie istniej¹cych rezerwacji dla fryzjera w danym tygodniu
            var reservations = _context.Reservation
                .Where(r => r.date >= startDate && r.date < startDate.AddDays(7) && r.HairdresserId == hairdresserId)
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
                    schedule[time][day] = new TimeBlock
                    {
                        StartTime = time,
                        EndTime = time.Add(new TimeSpan(0, 15, 0)),
                        IsReserved = true,
                        ClientInfo = $"{reservation.Client?.Name} {reservation.Client?.Surname}",
                        ServiceName = reservation.Service?.Name,
                        ServiceId = reservation.ServiceId
                    };
                }
            }

            return schedule;
        }
    }
}
