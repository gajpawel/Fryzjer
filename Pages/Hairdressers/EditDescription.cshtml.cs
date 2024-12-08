using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Fryzjer.Data;
using Fryzjer.Models;

namespace Fryzjer.Pages.Hairdressers
{
	public class EditDescriptionModel : PageModel
	{
		private readonly FryzjerContext _context;

		[BindProperty]
		public string? Description { get; set; }

		public EditDescriptionModel(FryzjerContext context)
		{
			_context = context;
		}

		public IActionResult OnGet()
		{
			// Pobranie ID fryzjera z sesji
			int? hairdresserId = HttpContext.Session.GetInt32("HairdresserId");
			if (hairdresserId == null)
			{
				return RedirectToPage("/Index"); // Je�li brak ID w sesji, przekierowanie na stron� g��wn�
			}

			// Pobranie fryzjera z bazy
			var hairdresser = _context.Hairdresser.FirstOrDefault(h => h.Id == hairdresserId.Value);
			if (hairdresser == null)
			{
				return RedirectToPage("/Index"); // Je�li brak fryzjera, przekierowanie na stron� g��wn�
			}

			// Przekazanie opisu do widoku
			Description = hairdresser.description;
			return Page();
		}

		public IActionResult OnPost()
		{
			// Pobranie ID fryzjera z sesji
			int? hairdresserId = HttpContext.Session.GetInt32("HairdresserId");
			if (hairdresserId == null)
			{
				return RedirectToPage("/Index"); // Je�li brak ID w sesji, przekierowanie na stron� g��wn�
			}

			// Pobranie fryzjera z bazy
			var hairdresser = _context.Hairdresser.FirstOrDefault(h => h.Id == hairdresserId.Value);
			if (hairdresser == null)
			{
				return RedirectToPage("/Index"); // Je�li brak fryzjera, przekierowanie na stron� g��wn�
			}

			// Aktualizacja opisu
			hairdresser.description = Description;
			_context.SaveChanges(); // Zapisanie zmian do bazy

			return RedirectToPage("/Hairdressers/HairdresserProfile"); // Przekierowanie po zapisaniu zmian
		}
	}
}
