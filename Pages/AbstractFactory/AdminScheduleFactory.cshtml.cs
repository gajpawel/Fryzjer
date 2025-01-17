using Fryzjer.Data;
using Fryzjer.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fryzjer.Pages.AbstractFactory
{
    // Konkretna implementacja fabryki dla harmonogramu administratora
    public class AdminScheduleFactory : ScheduleFactory
    {
        private readonly FryzjerContext _context;

        public AdminScheduleFactory(PageModel pageModel, FryzjerContext context) : base(pageModel)
        {
            _context = context;
        }

        public override IScheduleOperations CreateSchedule()
        {
            return new AdminScheduleOperations(_context);
        }

        private class AdminScheduleOperations : IScheduleOperations
        {
            private readonly FryzjerContext _context;

            public AdminScheduleOperations(FryzjerContext context)
            {
                _context = context;
            }

            public (List<DailySchedule>, List<DailySchedule>) CreateSchedule(int hairdresserId, DateTime startDate)
            {
                var weeklySchedule1 = GenerateScheduleInternalAsync(startDate, hairdresserId).Result;
                var weeklySchedule2 = GenerateScheduleInternalAsync(startDate.AddDays(7), hairdresserId).Result;
                return (weeklySchedule1, weeklySchedule2);
            }

            public void HandleReservation(int reservationId)
            {
                // Implementacja obs³ugi rezerwacji przez administratora
                var reservation = _context.Reservation.FirstOrDefault(r => r.Id == reservationId);
                if (reservation != null)
                {
                    reservation.status = 'C'; // Przyk³adowa zmiana statusu na zakoñczony
                    _context.SaveChanges();
                }
            }

            public void HandleVacationRequest(DateTime date, TimeSpan startTime, TimeSpan endTime)
            {
            }

            private async Task<List<DailySchedule>> GenerateScheduleInternalAsync(DateTime startDate, int hairdresserId)
            {
                var schedule = new List<DailySchedule>();

                for (int i = 0; i < 6; i++)
                {
                    var date = startDate.AddDays(i);
                    var reservations = await _context.Reservation
                        .Include(r => r.Client)
                        .Include(r => r.Service)
                        .Where(r => r.date.Date == date && r.HairdresserId == hairdresserId)
                        .ToListAsync();

                    var timeBlocks = reservations.OrderBy(r => r.time.TotalMinutes).Select(reservation => new TimeBlock
                    {
                        StartTime = reservation.time,
                        EndTime = reservation.time.Add(TimeSpan.FromMinutes(30)),
                        IsReserved = true,
                        ReservationId = reservation.Id,
                        ClientInfo = $"{reservation.Client?.Name} {reservation.Client?.Surname}\nTel: {reservation.Client?.Phone}",
                        ServiceName = reservation.Service?.Name ?? "Brak us³ugi",
                        Status = reservation.status
                    }).ToList();

                    schedule.Add(new DailySchedule
                    {
                        Date = date,
                        TimeBlocks = timeBlocks
                    });
                }

                return schedule;
            }
        }
    }
}
