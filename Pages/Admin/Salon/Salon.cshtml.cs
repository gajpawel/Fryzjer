using Microsoft.AspNetCore.Mvc.RazorPages;
using Fryzjer.Models;
using Fryzjer.Data;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace Fryzjer.Pages.Admin
{
    public class SalonModel : PageModel
    {
        private readonly FryzjerContext _context;

        public required List<Place> Places { get; set; }

        public SalonModel(FryzjerContext context)
        {
            _context = context;
        }

        // Metoda GET - ³adowanie listy salonów
        public void OnGet()
        {
            Places = _context.Place.ToList();
        }

        // Metoda POST - usuwanie salonu
        public async Task<IActionResult> OnPostAsync(int id)
        {
            var place = await _context.Place.FindAsync(id);

            if (place != null)
            {
                _context.Place.Remove(place);
                await _context.SaveChangesAsync();
            }

            // Odœwie¿ stronê, aby odzwierciedliæ zmiany
            return RedirectToPage("/Admin/Salon/Salon");
        }
    }
}
