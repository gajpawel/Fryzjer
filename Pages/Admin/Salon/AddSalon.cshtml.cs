using Fryzjer.Data;
using Fryzjer.Models;
using Fryzjer.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Fryzjer.Pages.Admin
{
    public class AddSalonModel : PageModel
    {
        private PlaceRepository _placeRepository;
        private readonly IWebHostEnvironment _environment;

        public AddSalonModel(FryzjerContext context, IWebHostEnvironment environment)
        {
            _environment = environment;
            _placeRepository = new PlaceRepository(context);
        }

        [BindProperty]
        public string Name { get; set; } = string.Empty;

        [BindProperty]
        public string Address { get; set; } = string.Empty;

        [BindProperty]
        public string TelephoneNumber { get; set; } = string.Empty;

        [BindProperty]
        public IFormFile? LogoFile { get; set; }

        [BindProperty]
        public IFormFile? PhotoFile { get; set; }

        [BindProperty]
        public string? Description { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var newSalon = new Place
            {
                Name = Name,
                address = Address,
                telephoneNumber = TelephoneNumber,
                description = Description
            };

            if (LogoFile != null)
            {
                var logoFileName = $"{Guid.NewGuid()}{Path.GetExtension(LogoFile.FileName)}";
                var logoPath = Path.Combine(_environment.WebRootPath, "images", logoFileName);
                using (var fileStream = new FileStream(logoPath, FileMode.Create))
                {
                    await LogoFile.CopyToAsync(fileStream);
                }
                newSalon.logoPath = $"/images/{logoFileName}";
            }

            if (PhotoFile != null)
            {
                var photoFileName = $"{Guid.NewGuid()}{Path.GetExtension(PhotoFile.FileName)}";
                var photoPath = Path.Combine(_environment.WebRootPath, "images", photoFileName);
                using (var fileStream = new FileStream(photoPath, FileMode.Create))
                {
                    await PhotoFile.CopyToAsync(fileStream);
                }
                newSalon.photoPath = $"/images/{photoFileName}";
            }

            _placeRepository.insert(newSalon);
            _placeRepository.save();

            return RedirectToPage("/Admin/Salon/Salon");
        }
    }
}

