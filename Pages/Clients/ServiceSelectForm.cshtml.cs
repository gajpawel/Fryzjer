using Fryzjer.Data;
using Fryzjer.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Fryzjer.Pages.AbstractFactory;

namespace Fryzjer.Pages.Clients
{
    public class ServiceSelectFormModel : PageModel
    {
        private readonly FryzjerContext _context;
        private readonly IScheduleOperations _scheduleOperations;

        // Harmonogram na bie¿¹cy tydzieñ
        public List<DailySchedule> WeeklySchedule1 { get; set; } = new List<DailySchedule>();

        // Harmonogram na kolejny tydzieñ
        public List<DailySchedule> WeeklySchedule2 { get; set; } = new List<DailySchedule>();
        public int CurrentWeek { get; set; } = 0;

        public ServiceSelectFormModel(FryzjerContext context)
        {
            _context = context;
            var factory = new ClientScheduleFactory(this, context);
            _scheduleOperations = factory.CreateSchedule();
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
        public string? ServiceName { get; private set; }
        public async Task<IActionResult> OnGetAsync(int id, int week = 0, int srv = 0)
        {
            ServiceId = srv;
            ClientId = HttpContext.Session.GetInt32("ClientId");

            // Tworzenie fabryki
            var factory = new ClientScheduleFactory(this, _context);

            // U¿ycie fabryki do utworzenia obiektu operacji na harmonogramie
            var scheduleOperations = factory.CreateSchedule();

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

            var factory = new ClientScheduleFactory(this, _context);
            var scheduleOperations = factory.CreateSchedule();

            // Rzutowanie do ClientScheduleOperations, aby uzyskaæ dostêp do specyficznych metod
            if (scheduleOperations is ClientScheduleOperations clientScheduleOperations)
            {
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
                        var startDate = DateTime.Now.Date.AddDays(7 * week - (int)DateTime.Now.DayOfWeek + 1);
                        (WeeklySchedule1, WeeklySchedule2) = clientScheduleOperations.CreateSchedule(hairdresser.Id, startDate);
                    }
                    else
                    {
                        ViewData["HairdresserDetails"] = "Nie znaleziono fryzjera.";
                    }
                }
            }
            else
            {
                ViewData["Error"] = "Nie uda³o siê utworzyæ operacji na harmonogramie.";
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
    }
}