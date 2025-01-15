using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Fryzjer.Data;
using Fryzjer.Models;
using System.Text.RegularExpressions;
using Fryzjer.Repositories;

namespace Fryzjer.Pages.Admin
{
    public class AddServiceModel : PageModel
    {
        private ServiceRepository _serviceRepository;

        [BindProperty]
        public Service NewService { get; set; } = new Service();

        public AddServiceModel(FryzjerContext context)
        {
            _serviceRepository = new ServiceRepository(context);
        }

        public void OnGet()
        {
            // Opcjonalnie mo¿esz przygotowaæ dane dla widoku
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Sprawdzenie formatu czasu
            var durationPattern = @"^\d{2}:\d{2}:\d{2}$"; // hh:mm:ss
            if (!Regex.IsMatch(NewService.Duration.ToString(), durationPattern))
            {
                ModelState.AddModelError("NewService.Duration", "Czas trwania musi byæ w formacie hh:mm:ss.");
                return Page();
            }

            // Dodanie nowej us³ugi do bazy danych
            _serviceRepository.insert(NewService);
            _serviceRepository.save();

            return RedirectToPage("/Admin/Services/Services");
        }
    }
}
