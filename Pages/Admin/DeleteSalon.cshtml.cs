using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Fryzjer.Models;
using Fryzjer.Data;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Fryzjer.Pages.Admin
{
    public class DeleteSalonModel : PageModel
    {
        private readonly FryzjerContext _context;

        public DeleteSalonModel(FryzjerContext context)
        {
            _context = context;
        }

        public Place Place { get; set; } = default!;

        // Wy�wietlanie strony potwierdzenia
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

        // Obs�uga ��dania POST (usuwanie salonu)
        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var place = await _context.Place.FindAsync(id);

            if (place != null)
            {
                _context.Place.Remove(place);

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateException ex)
                {
                    // Obs�uga b��du zwi�zana z ograniczeniami kluczy obcych
                    ModelState.AddModelError(string.Empty, "Nie mo�na usun�� salonu, poniewa� jest on powi�zany z innymi danymi.");
                    return Page(); // Ponowne wy�wietlenie strony
                }
            }

            return RedirectToPage("/Admin/Salon");
        }
    }
}
