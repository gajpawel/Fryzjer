using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Fryzjer.Data;
using Fryzjer.Models;

namespace Fryzjer.Pages.Hairdressers
{
    public class HairdresserProfileModel : PageModel
    {
        private readonly FryzjerContext _context;

        public Hairdresser? Hairdresser { get; set; }

        public HairdresserProfileModel(FryzjerContext context)
        {
            _context = context;
        }

        public IActionResult OnGet()
        {
            // Sprawdzenie ID fryzjera w sesji
            int? hairdresserId = HttpContext.Session.GetInt32("HairdresserId");
            if (hairdresserId == null)
            {
                // Je�li brak ID w sesji, przekierowanie na stron� g��wn�
                return RedirectToPage("/Index");
            }

            // Pobranie danych fryzjera z bazy na podstawie ID wraz z lokalem
            Hairdresser = _context.Hairdresser
                .Include(h => h.Place) // �adowanie powi�zanej lokalizacji
                .FirstOrDefault(h => h.Id == hairdresserId.Value);

            if (Hairdresser == null)
            {
                // Je�li fryzjer nie istnieje w bazie danych
                return RedirectToPage("/Index");
            }

            // Je�li wszystkie dane s� poprawne, wy�wietlenie profilu
            return Page();
        }
    }
}