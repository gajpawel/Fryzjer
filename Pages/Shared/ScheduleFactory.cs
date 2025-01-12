using Fryzjer.Data;
using Fryzjer.Models;
using Fryzjer.Pages.AbstractFactory;
using Microsoft.EntityFrameworkCore;

namespace Fryzjer.Pages.Shared
{
    public class ScheduleFactory
    {
        private readonly FryzjerContext _context;

        public ScheduleFactory(FryzjerContext context)
        {
            _context = context;
        }

        public async Task<List<ScheduleManagementModel.DailySchedule>> GenerateScheduleAsync(int hairdresserId, DateTime weekStartDate)
        {
            var schedule = new List<ScheduleManagementModel.DailySchedule>();

            for (int i = 0; i < 6; i++) // Tydzień pracy od poniedziałku do soboty
            {
                var date = weekStartDate.AddDays(i);
                var reservations = await _context.Reservation
                    .Include(r => r.Client)
                    .Include(r => r.Service)
                    .Where(r => r.date.Date == date && r.HairdresserId == hairdresserId)
                    .ToListAsync();

                var timeBlocks = new List<ScheduleManagementModel.TimeBlock>();
                foreach (var reservation in reservations.OrderBy(r => r.time.TotalMinutes))
                {
                    var serviceName = await _context.Service
                        .Where(s => s.Id == reservation.ServiceId)
                        .Select(s => s.Name)
                        .FirstOrDefaultAsync() ?? "Brak usługi";

                    timeBlocks.Add(new ScheduleManagementModel.TimeBlock
                    {
                        StartTime = reservation.time,
                        EndTime = reservation.time.Add(TimeSpan.FromMinutes(30)),
                        IsReserved = true,
                        ReservationId = reservation.Id,
                        ClientInfo = $"{reservation.Client?.Name} {reservation.Client?.Surname}\nTel: {reservation.Client?.Phone}",
                        ServiceName = serviceName,
                        Status = reservation.status
                    });
                }

                schedule.Add(new ScheduleManagementModel.DailySchedule
                {
                    Date = date,
                    TimeBlocks = timeBlocks
                });
            }

            return schedule;
        }

    }
}