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
            EditService = _context.Service.Find(id); // Pobranie istniej�cej us�ugi na podstawie ID
            if (EditService == null)
            {
                return RedirectToPage("/Admin/Services/Services"); // Je�li nie ma takiej us�ugi, przekierowanie do listy us�ug
            }
            return Page(); // Pokazuje formularz z danymi us�ugi
        }

        public IActionResult OnPost(int id)
        {
            if (!ModelState.IsValid)
            {
                // Obs�uguje b��dy walidacji
                ModelState.AddModelError("EditService.Price", "Cena nie jest poprawna.");
                return Page(); // Powr�t do formularza, z b��dami walidacji
            }

            // Sprawdzenie formatu czasu
            var durationPattern = @"^\d{2}:\d{2}:\d{2}$"; // hh:mm:ss
            if (!Regex.IsMatch(EditService.Duration.ToString(), durationPattern))
            {
                ModelState.AddModelError("EditService.Duration", "Czas trwania musi by� w formacie hh:mm:ss.");
                return Page();
            }

            // Pobranie us�ugi z bazy na podstawie ID
            var serviceToUpdate = _context.Service.Find(id);
            if (serviceToUpdate == null)
            {
                return NotFound(); // Je�li rekord nie istnieje, zwr�� status 404
            }

            // Aktualizacja p�l obiektu
            serviceToUpdate.Name = EditService.Name;
            serviceToUpdate.Duration = EditService.Duration;
            serviceToUpdate.Price = EditService.Price;

            _context.SaveChanges(); // Zapisanie zmian

            TempData["SuccessMessage"] = "Us�uga zosta�a zaktualizowana pomy�lnie!";

            return RedirectToPage("/Admin/Services/Services"); // Przekierowanie do listy us�ug po zapisaniu
        
        }

    }
}
