using Microsoft.AspNetCore.Mvc.RazorPages;
using Fryzjer.Models;
using Fryzjer.Data;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fryzjer.Pages.Admin
{
    public class SalonModel : PageModel
    {
        private readonly FryzjerContext _context;

        // Lista salonów
        public List<Place> Places { get; set; }

        public SalonModel(FryzjerContext context)
        {
            _context = context;
        }

        // Metoda GET - ³adowanie listy salonów
        public void OnGet()
        {
            // Pobieramy wszystkie salony, w tym nowe pola photoPath i description
            Places = _context.Place.ToList();
        }

        // Metoda POST - usuwanie salonu
        public async Task<IActionResult> OnPostAsync(int id)
        {
            // Szukamy salonu po id
            var place = await _context.Place.FindAsync(id);

            if (place != null)
            {
                // Usuwamy salon z bazy
                _context.Place.Remove(place);
                await _context.SaveChangesAsync();
            }

            // Po usuniêciu przekierowujemy na stronê z list¹ salonów
            return RedirectToPage("/Admin/Salon/Salon");
        }
    }
}
