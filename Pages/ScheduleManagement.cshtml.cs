// ScheduleManagement.cshtml.cs
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Fryzjer.Data;
using Fryzjer.Models;
using Microsoft.AspNetCore.Mvc;

namespace Fryzjer.Pages
{
    public class ScheduleManagementModel : PageModel
    {
        private readonly FryzjerContext _context;
        private readonly ILogger<ScheduleManagementModel> _logger;

        public ScheduleManagementModel(FryzjerContext context, ILogger<ScheduleManagementModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        [BindProperty(SupportsGet = true)]
        public int WeekOffset { get; set; } = 0;

        [BindProperty]
        public Hairdresser? SelectedHairdresser { get; set; }

        [BindProperty]
        public int? SelectedHairdresserId { get; set; }

        public SelectList HairdressersList { get; set; }

        public List<DailySchedule> WeeklySchedule { get; set; } = new();

        public DateTime WeekStartDate
        {
            get
            {
                var date = DateTime.Now.Date;
                // Cofamy się do poniedziałku
                while (date.DayOfWeek != DayOfWeek.Monday)
                {
                    date = date.AddDays(-1);
                }
                // Dodajemy offset tygodni
                return date.AddDays(WeekOffset * 7);
            }
        }

        public async Task<IActionResult> OnGetAsync(int? hairdresserId = null, int weekOffset = 0)
        {
            WeekOffset = weekOffset;

            var hairdressers = await _context.Hairdresser
                .OrderBy(h => h.Name)
                .ThenBy(h => h.Surname)
                .ToListAsync();

            HairdressersList = new SelectList(
                hairdressers.Select(h => new
                {
                    h.Id,
                    FullName = $"{h.Name} {h.Surname}"
                }),
                "Id",
                "FullName"
            );

            SelectedHairdresserId = hairdresserId ?? hairdressers.FirstOrDefault()?.Id;

            if (SelectedHairdresserId.HasValue)
            {
                SelectedHairdresser = await _context.Hairdresser
                    .Include(h => h.Place)
                    .FirstOrDefaultAsync(h => h.Id == SelectedHairdresserId);

                WeeklySchedule = await GenerateScheduleAsync(SelectedHairdresserId.Value);
            }

            return Page();
        }

        private async Task<List<DailySchedule>> GenerateScheduleAsync(int hairdresserId)
        {
            var schedule = new List<DailySchedule>();
            var startDate = WeekStartDate; // Używamy wyliczonej daty początku tygodnia

            while (startDate.DayOfWeek != DayOfWeek.Monday)
            {
                startDate = startDate.AddDays(-1);
            }

            // Generuj harmonogram od poniedziałku do soboty (6 dni), można zmienić na 5 albo 7 zależy ile dni w tygodniu lokal pracuje
            for (int i = 0; i < 6; i++)
            {
                var date = startDate.AddDays(i);
                var reservations = await _context.Reservation
                    .Include(r => r.Client)
                    .Include(r => r.Service)
                    .Where(r => r.date.Date == date && r.HairdresserId == hairdresserId)
                    .ToListAsync();

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

                        var serviceName = await _context.Service
                            .Where(s => s.Id == reservation.ServiceId)
                            .Select(s => s.Name)
                            .FirstOrDefaultAsync() ?? "Brak usługi";

                        currentBlock = new TimeBlock
                        {
                            StartTime = reservation.time,
                            EndTime = reservation.time.Add(TimeSpan.FromMinutes(30)), // Zwiększyłem slot do 30 minut
                            IsReserved = true,
                            ReservationId = reservation.Id,
                            ClientInfo = $"{reservation.Client?.Name} {reservation.Client?.Surname}\nTel: {reservation.Client?.Phone}",
                            ServiceName = serviceName,
                            Status = reservation.status
                        };
                    }
                    else
                    {
                        currentBlock.EndTime = currentBlock.EndTime.Add(TimeSpan.FromMinutes(30));
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
}