using Microsoft.AspNetCore.Mvc.RazorPages;
using Fryzjer.Models;
using Fryzjer.Data;
using Microsoft.AspNetCore.Mvc;

namespace Fryzjer.Pages.Admin
{
    public class SalonModel(FryzjerContext context) : PageModel
    {
        private readonly FryzjerContext _context = context;

        public required List<Place> Places { get; set; }

        public void OnGet()
        {
            Places = _context.Place.ToList();
        }
        public IActionResult OnGetAddSalon()
        {
            return RedirectToPage("/Admin/AddSalon"); // Przekierowanie na stronê tworzenia nowego salonu
        }
    }
}

