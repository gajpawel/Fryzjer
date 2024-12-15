using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Fryzjer.Data;
using Fryzjer.Models;
using System.Text.RegularExpressions;

namespace Fryzjer.Pages.Admin
{
    public class EditServiceModel : PageModel
    {
        private readonly FryzjerContext _context;

        [BindProperty]
        public Service EditService { get; set; } = new Service();

        public EditServiceModel(FryzjerContext context)
        {
            _context = context;
        }

        public IActionResult OnGet(int id)
        {
            EditService = _context.Service.Find(id); // Pobranie istniej¹cej us³ugi na podstawie ID
            if (EditService == null)
            {
                return RedirectToPage("/Admin/Services/Services"); // Jeœli nie ma takiej us³ugi, przekierowanie do listy us³ug
            }
            return Page(); // Pokazuje formularz z danymi us³ugi
        }

        public IActionResult OnPost(int id)
        {
            if (!ModelState.IsValid)
            {
                // Obs³uguje b³êdy walidacji
                ModelState.AddModelError("EditService.Price", "Cena nie jest poprawna.");
                return Page(); // Powrót do formularza, z b³êdami walidacji
            }

            // Sprawdzenie formatu czasu
            var durationPattern = @"^\d{2}:\d{2}:\d{2}$"; // hh:mm:ss
            if (!Regex.IsMatch(EditService.Duration.ToString(), durationPattern))
            {
                ModelState.AddModelError("EditService.Duration", "Czas trwania musi byæ w formacie hh:mm:ss.");
                return Page();
            }

            // Pobranie us³ugi z bazy na podstawie ID
            var serviceToUpdate = _context.Service.Find(id);
            if (serviceToUpdate == null)
            {
                return NotFound(); // Jeœli rekord nie istnieje, zwróæ status 404
            }

            // Aktualizacja pól obiektu
            serviceToUpdate.Name = EditService.Name;
            serviceToUpdate.Duration = EditService.Duration;
            serviceToUpdate.Price = EditService.Price;

            _context.SaveChanges(); // Zapisanie zmian

            TempData["SuccessMessage"] = "Us³uga zosta³a zaktualizowana pomyœlnie!";

            return RedirectToPage("/Admin/Services/Services"); // Przekierowanie do listy us³ug po zapisaniu
        
        }

    }
}
