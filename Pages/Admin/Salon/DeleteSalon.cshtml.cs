using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Fryzjer.Data;
using Fryzjer.Models;
using System.IO;

namespace Fryzjer.Pages
{
    public class DeleteSalonModel : PageModel
    {
        private readonly FryzjerContext _context;

        public DeleteSalonModel(FryzjerContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Place Place { get; set; }

        public IActionResult OnGet(int id)
        {
            Place = _context.Place.FirstOrDefault(p => p.Id == id);

            if (Place == null)
            {
                return NotFound();
            }

            return Page();
        }

        public IActionResult OnPost()
        {
            var placeToDelete = _context.Place.FirstOrDefault(p => p.Id == Place.Id);

            if (placeToDelete == null)
            {
                return NotFound();
            }

            // Usuniêcie pliku loga, jeœli istnieje
            if (!string.IsNullOrEmpty(placeToDelete.logoPath))
            {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", placeToDelete.logoPath.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }

            // Usuniêcie rekordu z bazy danych
            _context.Place.Remove(placeToDelete);
            _context.SaveChanges();

            return RedirectToPage("/Admin/Salon/Salon");
        }
    }
}
