using Fryzjer.Data;
using Fryzjer.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Fryzjer.Pages.Clients
{
    public class ServiceSelectFormModel : PageModel
    {
        private readonly FryzjerContext _context;

        // Harmonogram na bie¿¹cy tydzieñ
        public List<DailySchedule> WeeklySchedule1 { get; set; } = new List<DailySchedule>();

        // Harmonogram na kolejny tydzieñ
        public List<DailySchedule> WeeklySchedule2 { get; set; } = new List<DailySchedule>();

        [BindProperty]
        public string SelectedHour { get; set; }  // Godzina
        [BindProperty]
        public string SelectedDate { get; set; }  // Data

        public int CurrentWeek { get; set; } = 0;

        public ServiceSelectFormModel(FryzjerContext context)
        {
            _context = context;
        }

        [BindProperty]
        public int? SelectedHairdresserId { get; set; }

        [BindProperty]
        public int ServiceId { get; set; }

        [BindProperty]
        public String? SelectedHairdresserName { get; set; }

        public string PlaceName { get; set; }
        public List<string> HairdresserNames { get; set; } = new List<string>();
        public List<Service> Services { get; set; } = new List<Service>();

        public int? ClientId { get; set; }

        [BindProperty]
        public string Date { get; set; }

        [BindProperty]
        public string Time { get; set; }

        public string? ServiceName { get; private set; }

        public List<string> GetTimeSlots()
        {
            var timeSlots = new List<string>();
            for (int hour = 8; hour < 18; hour++)
            {
                timeSlots.Add($"{hour}:00");
                timeSlots.Add($"{hour}:15");
                timeSlots.Add($"{hour}:30");
                timeSlots.Add($"{hour}:45");
            }
            return timeSlots;
        }

        public async Task<IActionResult> OnGetAsync(int id, int week = 0, int srv = 0)
        {
            ServiceId = srv;
            ClientId = HttpContext.Session.GetInt32("ClientId");

            // Pobierz us³ugê z bazy danych
            var service = await _context.Service.FirstOrDefaultAsync(s => s.Id == srv);
            if (service != null)
            {
                ViewData["ServiceId"] = ServiceId;
                ViewData["ServiceName"] = service.Name;
                ViewData["ServiceDuration"] = service.Duration.ToString(@"hh\:mm");
            }

            // Wczytanie wspólnych danych
            await LoadDataForServiceAndHairdresser(id, week);

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id, int week = 0, int srv = 0)
        {
            ServiceId = srv;
            ViewData["ServiceId"] = ServiceId;

            // Pobierz informacje o us³udze
            var service = await _context.Service.FirstOrDefaultAsync(s => s.Id == srv);
            if (service != null)
            {
                ViewData["ServiceDuration"] = service.Duration.ToString(@"hh\:mm");
            }

            // Wczytanie wspólnych danych
            await LoadDataForServiceAndHairdresser(id, week);



            // Przetwarzanie logiki specyficznej dla POST
            if (!string.IsNullOrEmpty(SelectedHairdresserName))
            {
                var hairdresser = await _context.Hairdresser
                    .FirstOrDefaultAsync(h => (h.Name + " " + h.Surname) == SelectedHairdresserName);

                if (hairdresser != null)
                {
                    SelectedHairdresserId = hairdresser.Id;
                    ViewData["HairdresserDetails"] = $"Wybrano fryzjera: {hairdresser.Name} {hairdresser.Surname}";

                    var startDate = DateTime.Now.Date.AddDays(7 * week - (int)DateTime.Now.DayOfWeek + 1);
                    WeeklySchedule1 = await GenerateScheduleAsync(startDate, hairdresser.Id);
                    WeeklySchedule2 = await GenerateScheduleAsync(startDate.AddDays(7), hairdresser.Id);
                }
                else
                {
                    ViewData["HairdresserDetails"] = "Nie znaleziono fryzjera.";
                }
            }

            return Page();
        }

        private async Task LoadDataForServiceAndHairdresser(int id, int week)
        {
            // Wczytanie us³ugi
            var service = await _context.Service.FirstOrDefaultAsync(s => s.Id == ServiceId);
            ServiceName = service?.Name ?? "Nie znaleziono nazwy us³ugi.";
            ViewData["ServiceName"] = ServiceName;
            
            if (service != null)
            {
                ViewData["ServiceDuration"] = service.Duration.ToString(@"hh\:mm");
            }

            // Wczytanie miejsca
            var place = await _context.Place.FirstOrDefaultAsync(p => p.Id == id);
            PlaceName = place?.Name;

            // Wczytanie fryzjerów
            HairdresserNames = await _context.Specialization
                .Include(h => h.Hairdresser)
                .Include(s => s.Service)
                .Where(h => h.Hairdresser.PlaceId == id)
                .Where(s => s.Service.Name == ServiceName)
                .Select(h => $"{h.Hairdresser.Name} {h.Hairdresser.Surname}")
                .ToListAsync();

            if (!HairdresserNames.Any())
            {
                ViewData["Message"] = "Brak fryzjerów dla wybranego salonu.";
            }

            // Wczytanie us³ug
            Services = await _context.Specialization
                .Include(s => s.Service)
                .Where(s => s.Hairdresser.PlaceId == id)
                .Select(s => s.Service)
                .Distinct()
                .ToListAsync();

            if (!Services.Any())
            {
                ViewData["Message"] = "Brak us³ug dostêpnych dla tego salonu.";
            }

            // Ustawienie aktualnego tygodnia
            CurrentWeek = week;
            ViewData["SelectedHairdresserName"] = SelectedHairdresserName ?? "Nie wybrano fryzjera.";
        }

        private async Task<List<DailySchedule>> GenerateScheduleAsync(DateTime startDate, int hairdresserId)
        {
            var schedule = new List<DailySchedule>();

            for (int i = 0; i < 5; i++)
            {
                var date = startDate.AddDays(i);
                var timeBlocks = new List<TimeBlock>();

                if (date >= DateTime.Now.Date)
                {
                    timeBlocks = await GenerateDailyTimeBlocksAsync(date, hairdresserId);
                }

                schedule.Add(new DailySchedule
                {
                    Date = date,
                    TimeBlocks = timeBlocks
                });
            }

            return schedule;
        }

        private async Task<List<TimeBlock>> GenerateDailyTimeBlocksAsync(DateTime date, int hairdresserId)
        {
            var blocks = new List<TimeBlock>();
            var startTime = new TimeSpan(8, 0, 0);
            var endTime = new TimeSpan(18, 0, 0);

            var reservations = _context.Reservation
               .Include(r => r.Client)
               .Include(r => r.Service)
               .Where(r => r.date == date && r.HairdresserId == hairdresserId)
               .ToList()
               .OrderBy(r => r.time)
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
                        ClientInfo = "Zarezerwowane",
                        ServiceName = reservation.Service?.Name ?? "Brak us³ugi"
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

    public class DailySchedule
    {
        public DateTime Date { get; set; }
        public List<TimeBlock> TimeBlocks { get; set; } = new List<TimeBlock>();
    }

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