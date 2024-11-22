using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Fryzjer.Models;
using Fryzjer.Data;
using System.Threading.Tasks;

namespace Fryzjer.Pages.Admin
{
    public class AddSalonModel : PageModel
    {
        private readonly FryzjerContext _context;

        [BindProperty]
        public Place NewPlace { get; set; }

        public AddSalonModel(FryzjerContext context)
        {
            _context = context;
        }

        // Metoda OnGet() do wyœwietlenia formularza
        public void OnGet()
        {
        }

        // Metoda OnPost() do zapisania nowego salonu w bazie danych
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page(); // Jeœli dane s¹ niepoprawne, wróæ na stronê z formularzem
            }

            _context.Place.Add(NewPlace); // Dodajemy salon do bazy danych
            await _context.SaveChangesAsync(); // Zapisujemy zmiany w bazie

            // Po zapisaniu przekierowujemy u¿ytkownika na stronê z list¹ salonów
            return RedirectToPage("/Admin/Salon");
        }
    }
}
