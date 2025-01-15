using Fryzjer.Data;
using Fryzjer.Models;
using Fryzjer.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Fryzjer.Pages.Admin
{
    public class EditSalonModel : PageModel
    {
        private PlaceRepository _placeRepository;
        private readonly IWebHostEnvironment _environment;

        public EditSalonModel(FryzjerContext context, IWebHostEnvironment environment)
        {
            _placeRepository = new PlaceRepository(context);
            _environment = environment;
        }

        [BindProperty]
        public int Id { get; set; }

        [BindProperty]
        public string Name { get; set; } = string.Empty;

        [BindProperty]
        public string Address { get; set; } = string.Empty;

        [BindProperty]
        public string TelephoneNumber { get; set; } = string.Empty;

        [BindProperty]
        public string? Description { get; set; }

        [BindProperty]
        public IFormFile? LogoFile { get; set; }

        [BindProperty]
        public IFormFile? PhotoFile { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var salon = _placeRepository.getById(id);
            if (salon == null)
            {
                return NotFound();
            }

            Id = salon.Id;
            Name = salon.Name ?? string.Empty;
            Address = salon.address ?? string.Empty;
            TelephoneNumber = salon.telephoneNumber ?? string.Empty;
            Description = salon.description;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var salon = _placeRepository.getById(Id);
            if (salon == null)
            {
                return NotFound();
            }

            salon.Name = Name;
            salon.address = Address;
            salon.telephoneNumber = TelephoneNumber;
            salon.description = Description;

            if (LogoFile != null)
            {
                var logoFileName = $"{Guid.NewGuid()}{Path.GetExtension(LogoFile.FileName)}";
                var logoPath = Path.Combine(_environment.WebRootPath, "images", logoFileName);
                using (var fileStream = new FileStream(logoPath, FileMode.Create))
                {
                    await LogoFile.CopyToAsync(fileStream);
                }
                salon.logoPath = $"/images/{logoFileName}";
            }

            if (PhotoFile != null)
            {
                var photoFileName = $"{Guid.NewGuid()}{Path.GetExtension(PhotoFile.FileName)}";
                var photoPath = Path.Combine(_environment.WebRootPath, "images", photoFileName);
                using (var fileStream = new FileStream(photoPath, FileMode.Create))
                {
                    await PhotoFile.CopyToAsync(fileStream);
                }
                salon.photoPath = $"/images/{photoFileName}";
            }

            _placeRepository.save();

            return RedirectToPage("/Admin/Salon/Salon");
        }
    }
}

