using Fryzjer.Data;
using Fryzjer.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Fryzjer.Pages.Clients
{
    public class ServiceSelectFormModel : PageModel
    {
        private readonly FryzjerContext _context;

        // Harmonogram na bie¿¹cy tydzieñ
        public List<DailySchedule> WeeklySchedule1 { get; set; } = new List<DailySchedule>();

        // Harmonogram na kolejny tydzieñ
        public List<DailySchedule> WeeklySchedule2 { get; set; } = new List<DailySchedule>();

        public int CurrentWeek { get; set; } = 0;

        public ServiceSelectFormModel(FryzjerContext context)
        {
            _context = context;
        }

        [BindProperty]
        public int PlaceId { get; set; }

        [BindProperty]
        public int ServiceId { get; set; }

        [BindProperty]
        public String? SelectedHairdresserName { get; set; }

        public string SelectedServiceName { get; set; } // Nazwa wybranej us³ugi


        public string PlaceName { get; set; }
        public List<string> HairdresserNames { get; set; } = new List<string>();
        public List<Service> Services { get; set; } = new List<Service>();



        [BindProperty]
        public string Date { get; set; }

        [BindProperty]
        public string Time { get; set; }

        [BindProperty]
        public string HairdresserName { get; set; }
        public string? ServiceName { get; private set; }

        public List<string> GetTimeSlots()
        {
            var timeSlots = new List<string>();
            for (int hour = 8; hour < 18; hour++) // godziny od 8:00 do 17:45
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
            PlaceId = id;
            ServiceId = srv;

            // Przypisanie ServiceId do ViewData
            ViewData["ServiceId"] = ServiceId;

            var service = await _context.Service.FirstOrDefaultAsync(s => s.Id == ServiceId);
            if (service != null)
            {
                ServiceName = service.Name; // Przypisanie nazwy us³ugi do zmiennej
                ViewData["ServiceName"] = ServiceName; // Przekazanie do ViewData
            }
            else
            {
                ServiceName = "Nie znaleziono nazwy us³ugi."; // W przypadku, gdy nie uda³o siê znaleŸæ nazwy us³ugi
            }

            var place = await _context.Place.FirstOrDefaultAsync(p => p.Id == id);
            if (place != null)
            {
                PlaceName = place.Name;
            }

            HairdresserNames = await _context.Specialization
                .Include(h => h.Hairdresser)
                .Include(s => s.Service)
                .Where(h => h.Hairdresser.PlaceId == id)
                .Where(s => s.Service.Name == ServiceName)
                .Select(h => h.Hairdresser.Name + " " + h.Hairdresser.Surname)
                .ToListAsync();

            if (!HairdresserNames.Any())
            {
                ViewData["Message"] = "Brak fryzjerów dla wybranego salonu.";
            }

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

            CurrentWeek = week; // Correctly assign week here

            ViewData["SelectedHairdresserName"] = SelectedHairdresserName ?? "Nie wybrano fryzjera.";

            return Page();
        }




        public async Task<IActionResult> OnPostAsync(int id, int week = 0, int srv = 0) {

            ServiceId = srv;

            ViewData["ServiceId"] = ServiceId;

            var service = await _context.Service.FirstOrDefaultAsync(s => s.Id == ServiceId);
             SelectedServiceName = service?.Name ?? "Nie wybrano us³ugi";


            var place = await _context.Place.FirstOrDefaultAsync(p => p.Id == id);
            if (place != null)
            {
                PlaceName = place.Name;
            }


            if (!string.IsNullOrEmpty(SelectedHairdresserName))
            {
                ViewData["SelectedHairdresserName"] = SelectedHairdresserName; // Zachowujemy wybór fryzjera
            }

            // Opcjonalnie: odczytanie szczegó³ów wybranego fryzjera
            if (!string.IsNullOrEmpty(SelectedHairdresserName))
            {
                var hairdresser = await _context.Hairdresser
                    .FirstOrDefaultAsync(h => (h.Name + " " + h.Surname) == SelectedHairdresserName);


                CurrentWeek = week; // Correctly assign week here


                if (hairdresser != null)
                {
                    ViewData["HairdresserDetails"] = $"Wybrano fryzjera: {hairdresser.Name} {hairdresser.Surname}";

                    // Generowanie harmonogramu dla wybranego fryzjera
                    var startDate = DateTime.Now.Date.AddDays(7 * week - (int)DateTime.Now.DayOfWeek + 1); // +1 to adjust for Monday as the start
                    WeeklySchedule1 = await GenerateScheduleAsync(startDate, hairdresser.Id); // Bie¿¹cy tydzieñ
                    WeeklySchedule2 = await GenerateScheduleAsync(startDate.AddDays(7), hairdresser.Id); // Kolejny tydzieñ
                }
                else
                {
                    ViewData["HairdresserDetails"] = "Nie znaleziono fryzjera.";
                }
            }

            return Page(); // Powrót do widoku
        }



        public async Task<IActionResult> OnPostReserveAsync()
        {
            // Pobranie aktualnego u¿ytkownika (przyk³ad dla Identity)
            var clientId = User.FindFirstValue(ClaimTypes.NameIdentifier); // Zak³adamy, ¿e NameIdentifier to ClientId

            if (string.IsNullOrEmpty(clientId))
            {
                TempData["Error"] = "Nie znaleziono zalogowanego u¿ytkownika.";
                return RedirectToPage("/Clients/ClientProfile");
            }

            // SprawdŸ, czy wybrano wszystkie dane
            if (string.IsNullOrEmpty(Date) || string.IsNullOrEmpty(Time) || ServiceId == 0 || HairdresserName == null)
            {
                TempData["Error"] = "Proszê uzupe³niæ wszystkie dane rezerwacji.";
                return Page();
            }

            // ZnajdŸ fryzjera na podstawie imienia i nazwiska
            var hairdresser = await _context.Hairdresser
                .FirstOrDefaultAsync(h => (h.Name + " " + h.Surname) == HairdresserName);

            if (hairdresser == null)
            {
                TempData["Error"] = "Nie znaleziono wybranego fryzjera.";
                return Page();
            }

            // Przekierowanie bez zapisu do bazy
            TempData["Success"] = "Rezerwacja zosta³a zapisana.";
            return RedirectToPage("/Clients/ClientProfile");
        }




        private async Task<List<DailySchedule>> GenerateScheduleAsync(DateTime startDate, int hairdresserId)
        {
            var schedule = new List<DailySchedule>();

            for (int i = 0; i < 5; i++) // Tylko dni robocze (poniedzia³ek–pi¹tek)
            {
                var date = startDate.AddDays(i);
                var timeBlocks = new List<TimeBlock>();

                if (date >= DateTime.Now.Date) // Generujemy godziny tylko dla przysz³ych dat
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
            var startTime = new TimeSpan(8, 0, 0); // Start o 08:00
            var endTime = new TimeSpan(18, 0, 0); // Koniec o 18:00

            // Pobieramy istniej¹ce rezerwacje na dany dzieñ dla danego fryzjera
            var reservations = _context.Reservation
               .Include(r => r.Client)
               .Include(r => r.Service)
               .Where(r => r.date == date && r.HairdresserId == hairdresserId)
               .ToList()                  // Najpierw pobieramy dane
               .OrderBy(r => r.time)      // Potem sortujemy w pamiêci
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
                        ClientInfo = $"{reservation.Client?.Name} {reservation.Client?.Surname}\nTel: {reservation.Client?.Phone}",
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

    // Klasa do reprezentowania bloku czasowego
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
