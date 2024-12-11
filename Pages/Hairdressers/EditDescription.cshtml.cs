using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Fryzjer.Data;
using Fryzjer.Models;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace Fryzjer.Pages.Hairdressers
{
    public class EditDescriptionModel : PageModel
    {
        private readonly FryzjerContext _context;

        [BindProperty]
        public string? Description { get; set; }

        [BindProperty]
        public IFormFile? ProfilePhoto { get; set; }

        public string? ErrorMessage { get; set; } // Pole do przechowywania komunikatu o b³êdzie

        public EditDescriptionModel(FryzjerContext context)
        {
            _context = context;
        }

        public IActionResult OnGet()
        {
            int? hairdresserId = HttpContext.Session.GetInt32("HairdresserId");
            if (hairdresserId == null)
            {
                return RedirectToPage("/Index");
            }

            var hairdresser = _context.Hairdresser.FirstOrDefault(h => h.Id == hairdresserId.Value);
            if (hairdresser == null)
            {
                return RedirectToPage("/Index");
            }

            Description = hairdresser.description;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            int? hairdresserId = HttpContext.Session.GetInt32("HairdresserId");
            if (hairdresserId == null)
            {
                return RedirectToPage("/Index");
            }

            var hairdresser = _context.Hairdresser.FirstOrDefault(h => h.Id == hairdresserId.Value);
            if (hairdresser == null)
            {
                return RedirectToPage("/Index");
            }

            hairdresser.description = Description;

            // Walidacja zdjêcia
            if (ProfilePhoto != null && ProfilePhoto.Length > 0)
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
                var fileExtension = Path.GetExtension(ProfilePhoto.FileName).ToLowerInvariant();

                if (!allowedExtensions.Contains(fileExtension))
                {
                    ErrorMessage = "Dozwolone formaty plików to: .jpg, .jpeg, .png";
                    return Page(); // Powrót do strony w przypadku b³êdu
                }

                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Hairdresser_Photos");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                string fileName = $"{hairdresser.login}{fileExtension}";
                string filePath = Path.Combine(uploadsFolder, fileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await ProfilePhoto.CopyToAsync(fileStream);
                }

                hairdresser.photoPath = Path.Combine("Hairdresser_Photos", fileName);
            }

            await _context.SaveChangesAsync();
            return RedirectToPage("/Hairdressers/HairdresserProfile");
        }
    }
}
