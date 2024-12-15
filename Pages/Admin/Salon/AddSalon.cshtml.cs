using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Fryzjer.Models;
using Fryzjer.Data;
using System.IO;

namespace Fryzjer.Pages
{
    public class AddSalonModel : PageModel
    {
        private readonly FryzjerContext _context;

        public AddSalonModel(FryzjerContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Place Place { get; set; }

        [BindProperty]
        public IFormFile? LogoFile { get; set; } // Plik z logiem

        public void OnGet()
        {
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            if (LogoFile != null)
            {
                // Tworzenie œcie¿ki do zapisu pliku
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");
                Directory.CreateDirectory(uploadsFolder); // Upewnij siê, ¿e folder istnieje

                var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(LogoFile.FileName)}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                // Zapis pliku na serwerze
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    LogoFile.CopyTo(fileStream);
                }

                // Zapis œcie¿ki do loga
                Place.logoPath = $"/images/{uniqueFileName}";
            }

            // Zapis do bazy danych
            _context.Place.Add(Place);
            _context.SaveChanges();

            return RedirectToPage("/Admin/Salon/Salon");
        }
    }
}
