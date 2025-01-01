using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Fryzjer.Data;
using Fryzjer.Models;

namespace Fryzjer.Pages
{
    public class ScheduleManagement : PageModel
    {
        private readonly FryzjerContext _context;
        private readonly ILogger<ScheduleManagement> _logger;

        public ScheduleManagement(FryzjerContext context, ILogger<ScheduleManagement> logger)
        {
            _context = context;
            _logger = logger;
        }

        [BindProperty]
        public List<Hairdresser> Hairdressers { get; set; } = new();

        [BindProperty]
        public int? SelectedHairdresserId { get; set; }

        public SelectList HairdressersList { get; set; }

        public List<DailySchedule> WeeklySchedule { get; set; } = new();

        public class WorkHoursModel
        {
            public TimeSpan StartTime { get; set; } = new TimeSpan(8, 0, 0);
            public TimeSpan EndTime { get; set; } = new TimeSpan(18, 0, 0);
            public int HairdresserId { get; set; }
        }

        [BindProperty]
        public WorkHoursModel WorkHours { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int? hairdresserId = null)
        {

            Hairdressers = await _context.Hairdresser
                .Include(h => h.Place)
                .OrderBy(h => h.Name)
                .ToListAsync();

            HairdressersList = new SelectList(
    Hairdressers.Select(h => new
                {
                    h.Id,
                    FullName = $"{h.Name} {h.Surname}"
                }),
                "Id",
                "FullName");

            SelectedHairdresserId = hairdresserId ?? Hairdressers.FirstOrDefault()?.Id;

            if (SelectedHairdresserId.HasValue)
            {
                WeeklySchedule = await GenerateScheduleAsync(SelectedHairdresserId.Value);
            }

            return Page();
        }

        private async Task<List<DailySchedule>> GenerateScheduleAsync(int hairdresserId)
        {
            var schedule = new List<DailySchedule>();
            var startDate = DateTime.Now.Date;

            for (int i = 0; i < 5; i++)
            {
                var date = startDate.AddDays(i);
                var reservations = await _context.Reservation
                    .Include(r => r.Client)
                    .Include(r => r.Service)
                    .Where(r => r.date == date && r.HairdresserId == hairdresserId)
                    .ToListAsync();  // Najpierw pobieramy dane

                // Sortowanie po stronie klienta
                reservations = reservations.OrderBy(r => r.time.TotalMinutes).ToList();

                var timeBlocks = new List<TimeBlock>();
                TimeBlock currentBlock = null;

                foreach (var reservation in reservations)
                {
                    if (currentBlock == null || reservation.time != currentBlock.EndTime)
                    {
                        if (currentBlock != null)
                        {
                            timeBlocks.Add(currentBlock);
                        }
                        currentBlock = new TimeBlock
                        {
                            StartTime = reservation.time,
                            EndTime = reservation.time.Add(TimeSpan.FromMinutes(15)),
                            IsReserved = true,
                            ReservationId = reservation.Id,
                            ClientInfo = $"{reservation.Client?.Name} {reservation.Client?.Surname}\nTel: {reservation.Client?.Phone}",
                            ServiceName = reservation.Service?.Name ?? "Brak usługi",
                            Status = reservation.status
                        };
                    }
                    else
                    {
                        currentBlock.EndTime = currentBlock.EndTime.Add(TimeSpan.FromMinutes(15));
                    }
                }

                if (currentBlock != null)
                {
                    timeBlocks.Add(currentBlock);
                }

                schedule.Add(new DailySchedule
                {
                    Date = date,
                    TimeBlocks = timeBlocks
                });
            }

            return schedule;
        }

        public async Task<IActionResult> OnPostUpdateWorkHoursAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var hairdresser = await _context.Hairdresser.FindAsync(WorkHours.HairdresserId);
            if (hairdresser == null)
            {
                return NotFound();
            }

            await _context.SaveChangesAsync();

            return RedirectToPage(new { hairdresserId = WorkHours.HairdresserId });
        }
    }

    public class DailySchedule
    {
        public DateTime Date { get; set; }
        public List<TimeBlock> TimeBlocks { get; set; } = new();
    }

    public class TimeBlock
    {
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public bool IsReserved { get; set; }
        public string TimeRange => $"{StartTime.Hours:00}:{StartTime.Minutes:00} - {EndTime.Hours:00}:{EndTime.Minutes:00}";
        public string? ClientInfo { get; set; }
        public int? ReservationId { get; set; }
        public string? ServiceName { get; set; }
        public char? Status { get; set; }
    }
}
