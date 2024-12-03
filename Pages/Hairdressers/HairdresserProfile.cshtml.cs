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
                // Jeœli brak ID w sesji, przekierowanie na stronê g³ówn¹
                return RedirectToPage("/Index");
            }

            // Pobranie danych fryzjera z bazy na podstawie ID wraz z lokalem
            Hairdresser = _context.Hairdresser
                .Include(h => h.Place) // £adowanie powi¹zanej lokalizacji
                .FirstOrDefault(h => h.Id == hairdresserId.Value);

            if (Hairdresser == null)
            {
                // Jeœli fryzjer nie istnieje w bazie danych
                return RedirectToPage("/Index");
            }

            // Jeœli wszystkie dane s¹ poprawne, wyœwietlenie profilu
            return Page();
        }
    }
}