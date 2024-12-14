using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Fryzjer.Data;
using Fryzjer.Models;

namespace Fryzjer.Pages.Admin
{
    public class EditSalonModel : PageModel
    {
        private readonly FryzjerContext _context;

        [BindProperty]
        public Place Place { get; set; } = default!;

        public EditSalonModel(FryzjerContext context)
        {
            _context = context;
        }

        // GET - Pobieranie istniej¹cych danych salonu
        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Place = await _context.Place.FindAsync(id);

            if (Place == null)
            {
                return NotFound();
            }

            return Page();
        }

        // POST - Zapisywanie edytowanych danych
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var placeToUpdate = await _context.Place.FindAsync(Place.Id);

            if (placeToUpdate == null)
            {
                return NotFound();
            }

            // Aktualizacja pól
            placeToUpdate.Name = Place.Name;
            placeToUpdate.address = Place.address;
            placeToUpdate.logoPath = Place.logoPath;
            placeToUpdate.telephoneNumber = Place.telephoneNumber;

            await _context.SaveChangesAsync();

            return RedirectToPage("/Admin/Salon/Salon"); // Powrót do listy salonów
        }
    }
}
