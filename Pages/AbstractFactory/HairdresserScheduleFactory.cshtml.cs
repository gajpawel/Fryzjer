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

        public HairdresserScheduleFactoryModel(FryzjerContext context)
        {
            _context = context;
        }

        public override void GenerateSchedule(int week)
        {
            // Pobieramy ID zalogowanego fryzjera
            int? hairdresserId = HttpContext.Session.GetInt32("HairdresserId");
            if (hairdresserId == null)
            {
                // Jeœli fryzjer nie jest zalogowany, przekierowujemy na stronê logowania
                Response.Redirect("/Login");
                return;
            }

            CurrentWeek = week;

            // Obliczamy pierwszy dzieñ tygodnia (poniedzia³ek)
            var startDate = DateTime.Now.Date.AddDays(7 * week - (int)DateTime.Now.DayOfWeek + 1);

            // Generujemy harmonogram tygodniowy (poniedzia³ek–pi¹tek)
            WeeklySchedule = GenerateWeeklySchedule(startDate, hairdresserId.Value);
        }

        private List<DailySchedule> GenerateWeeklySchedule(DateTime startDate, int hairdresserId)
        {
            var schedule = new List<DailySchedule>();

            for (int i = 0; i < 5; i++) // Tylko dni robocze (poniedzia³ek–pi¹tek)
            {
                var date = startDate.AddDays(i);
                var timeBlocks = new List<TimeBlock>();

                if (date >= DateTime.Now.Date) // Generujemy godziny tylko dla przysz³ych dat
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

            // Pobieramy istniej¹ce rezerwacje
            var reservations = _context.Reservation
                .Where(r => r.date == date && r.HairdresserId == hairdresserId)
                .OrderBy(r => r.time) // Sortowanie rezerwacji
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
                            ? $"{reservation.Client.Name} {reservation.Client.Surname}, Tel: {reservation.Client.Phone}, Us³uga: {reservation.Service?.Name}"
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
}
