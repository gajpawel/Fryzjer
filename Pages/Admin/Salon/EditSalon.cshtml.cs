using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Fryzjer.Models;
using Fryzjer.Data;
using System.IO;

namespace Fryzjer.Pages
{
    public class EditSalonModel : PageModel
    {
        private readonly FryzjerContext _context;

        public EditSalonModel(FryzjerContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Place Place { get; set; }

        [BindProperty]
        public IFormFile? LogoFile { get; set; }

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
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var placeToUpdate = _context.Place.FirstOrDefault(p => p.Id == Place.Id);

            if (placeToUpdate == null)
            {
                return NotFound();
            }

            // Aktualizacja loga, jeœli u¿ytkownik przes³a³ nowy plik
            if (LogoFile != null)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");
                Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(LogoFile.FileName)}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                // Usuniêcie starego pliku
                if (!string.IsNullOrEmpty(placeToUpdate.logoPath))
                {
                    var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", placeToUpdate.logoPath.TrimStart('/'));
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                }

                // Zapis nowego pliku
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    LogoFile.CopyTo(fileStream);
                }

                placeToUpdate.logoPath = $"/images/{uniqueFileName}";
            }

            // Aktualizacja pozosta³ych pól
            placeToUpdate.Name = Place.Name;
            placeToUpdate.address = Place.address;
            placeToUpdate.telephoneNumber = Place.telephoneNumber;

            _context.SaveChanges();
            return RedirectToPage("/Admin/Salon/Salon");
        }
    }
}

