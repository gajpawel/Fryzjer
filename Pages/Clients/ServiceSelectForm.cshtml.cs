using Fryzjer.Data;
using Fryzjer.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fryzjer.Pages.Clients
{
    public class ServiceSelectFormModel : PageModel
    {
        private readonly FryzjerContext _context;

        public ServiceSelectFormModel(FryzjerContext context)
        {
            _context = context;
        }

        [BindProperty]
        public int PlaceId { get; set; }

        public string PlaceName { get; set; }
        public List<string> HairdresserNames { get; set; } = new List<string>();
        public List<Service> Services { get; set; } = new List<Service>();

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

        public async Task<IActionResult> OnGetAsync(int id)
        {
            PlaceId = id;

            var place = await _context.Place.FirstOrDefaultAsync(p => p.Id == id);
            if (place != null)
            {
                PlaceName = place.Name;
            }

            HairdresserNames = await _context.Hairdresser
                .Where(h => h.PlaceId == id)
                .Select(h => h.Name + " " + h.Surname)
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

            return Page();
        }
    }
}
