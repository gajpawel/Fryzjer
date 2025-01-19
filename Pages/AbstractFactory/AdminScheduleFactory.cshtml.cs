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
                var reservation = _context.Reservation.FirstOrDefault(r => r.Id == reservationId);
                if (reservation != null)
                {
                    reservation.status = 'C';
                    _context.SaveChanges();
                }
            }

            public void HandleVacationRequest(DateTime date, TimeSpan startTime, TimeSpan endTime)
            {
            }

            private async Task<List<DailySchedule>> GenerateScheduleInternalAsync(DateTime startDate, int hairdresserId)
            {
                var schedule = new List<DailySchedule>();
                var vacationService = await _context.Service.FirstOrDefaultAsync(s => s.Name.ToLower() == "urlop");

                for (int i = 0; i < 6; i++)
                {
                    var date = startDate.AddDays(i);

                    // Get reservations without ordering first
                    var reservations = await _context.Reservation
                        .Include(r => r.Client)
                        .Include(r => r.Service)
                        .Where(r => r.date.Date == date && r.HairdresserId == hairdresserId)
                        .ToListAsync();

                    // Order in memory
                    reservations = reservations
                        .OrderBy(r => r.time.TotalMinutes)
                        .ToList();

                    var timeBlocks = new List<TimeBlock>();
                    TimeBlock currentBlock = null;
                    TimeBlock currentVacationBlock = null;

                    foreach (var reservation in reservations)
                    {
                        bool isVacation = (vacationService != null && reservation.ServiceId == vacationService.Id);

                        if (isVacation)
                        {
                            if (currentVacationBlock == null ||
                                reservation.time != currentVacationBlock.EndTime ||
                                reservation.status != currentVacationBlock.Status)
                            {
                                if (currentVacationBlock != null)
                                    timeBlocks.Add(currentVacationBlock);

                                currentVacationBlock = new TimeBlock
                                {
                                    StartTime = reservation.time,
                                    EndTime = reservation.time.Add(TimeSpan.FromMinutes(15)),
                                    IsReserved = true,
                                    ReservationId = reservation.Id,
                                    ClientInfo = $"Urlop ({GetStatusText(reservation.status)})",
                                    ServiceName = "Urlop",
                                    Status = reservation.status
                                };
                            }
                            else
                            {
                                currentVacationBlock.EndTime = currentVacationBlock.EndTime.Add(TimeSpan.FromMinutes(15));
                            }
                        }
                        else
                        {
                            bool startNew = ShouldStartNewBlock(currentBlock, reservation);

                            if (startNew)
                            {
                                if (currentBlock != null)
                                    timeBlocks.Add(currentBlock);

                                currentBlock = new TimeBlock
                                {
                                    StartTime = reservation.time,
                                    EndTime = reservation.time.Add(TimeSpan.FromMinutes(15)),
                                    IsReserved = true,
                                    ReservationId = reservation.Id,
                                    ClientInfo = FormatClientInfo(reservation),
                                    ServiceName = reservation.Service?.Name ?? "Brak us³ugi",
                                    Status = reservation.status
                                };
                            }
                            else
                            {
                                currentBlock.EndTime = currentBlock.EndTime.Add(TimeSpan.FromMinutes(15));
                            }
                        }
                    }

                    if (currentBlock != null)
                        timeBlocks.Add(currentBlock);
                    if (currentVacationBlock != null)
                        timeBlocks.Add(currentVacationBlock);

                    schedule.Add(new DailySchedule
                    {
                        Date = date,
                        TimeBlocks = timeBlocks
                    });
                }

                return schedule;
            }

            private bool ShouldStartNewBlock(TimeBlock currentBlock, Reservation reservation)
            {
                if (currentBlock == null)
                    return true;

                return reservation.time != currentBlock.EndTime ||
                       reservation.status != currentBlock.Status ||
                       reservation.ClientId != currentBlock.ClientId ||
                       reservation.ServiceId != currentBlock.ServiceId;
            }

            private string FormatClientInfo(Reservation reservation)
            {
                if (reservation.Client == null)
                    return "Zarezerwowane";

                var info = $"{reservation.Client.Name} {reservation.Client.Surname}\nTel: {reservation.Client.Phone}";
                if (reservation.status == 'Z')
                    info += "\n(Zakoñczone)";

                return info;
            }

            private string GetStatusText(char status) => status switch
            {
                'O' => "Oczekuj¹cy",
                'P' => "Potwierdzony",
                'A' => "Anulowany",
                'Z' => "Zakoñczony",
                _ => "Nieznany"
            };
        }
    }
}