using Fryzjer.Data;
using Fryzjer.Models;
using Fryzjer.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System.IO;

namespace Fryzjer.Pages.Admin
{
    public class DeleteSalonModel : PageModel
    {
        private PlaceRepository _placeRepository;
        private readonly IWebHostEnvironment _environment;

        public DeleteSalonModel(FryzjerContext context, IWebHostEnvironment environment)
        {
            _placeRepository = new PlaceRepository(context);
            _environment = environment;
        }

        [BindProperty]
        public int Id { get; set; }

        public string Name { get; set; }
        public string Address { get; set; }
        public string TelephoneNumber { get; set; }
        public string? Description { get; set; }
        public string? LogoPath { get; set; }
        public string? PhotoPath { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var salon = _placeRepository.getById(id);
            if (salon == null)
            {
                return NotFound();
            }

            Name = salon.Name;
            Address = salon.address;
            TelephoneNumber = salon.telephoneNumber;
            Description = salon.description;
            LogoPath = salon.logoPath;
            PhotoPath = salon.photoPath;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            var salon = _placeRepository.getById(id);
            if (salon == null)
            {
                return NotFound();
            }

            // Usuñ logo (jeœli istnieje)
            if (!string.IsNullOrEmpty(salon.logoPath))
            {
                var logoPath = Path.Combine(_environment.WebRootPath, salon.logoPath.TrimStart('/'));
                if (System.IO.File.Exists(logoPath))
                {
                    System.IO.File.Delete(logoPath);
                }
            }

            // Usuñ zdjêcie lokalu (jeœli istnieje)
            if (!string.IsNullOrEmpty(salon.photoPath))
            {
                var photoPath = Path.Combine(_environment.WebRootPath, salon.photoPath.TrimStart('/'));
                if (System.IO.File.Exists(photoPath))
                {
                    System.IO.File.Delete(photoPath);
                }
            }

            // Usuñ salon z bazy
            _placeRepository.deleteById(salon.Id);
            _placeRepository.save();

            return RedirectToPage("/Admin/Salon/Salon");
        }
    }
}
