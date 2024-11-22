using Fryzjer.Data;
using Fryzjer.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

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

            int? hairdresserId = HttpContext.Session.GetInt32("HairdresserId");

            if (hairdresserId == null)
            {
                // Jeœli ID fryzjera nie istnieje w sesji, przekieruj na stronê g³ówn¹
                return RedirectToPage("/Index");
            }

            // Pobierz dane fryzjera z bazy danych
            Hairdresser = _context.Hairdresser.FirstOrDefault(h => h.Id == hairdresserId.Value);

            if (Hairdresser == null)
            {
                // Jeœli fryzjer nie istnieje w bazie, równie¿ przekieruj na stronê g³ówn¹
                return RedirectToPage("/Index");
            }

            // Jeœli wszystko jest poprawne, wyœwietl profil fryzjera
            return Page();
        }

    }
}
