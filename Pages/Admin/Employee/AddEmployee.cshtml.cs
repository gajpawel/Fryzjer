using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Fryzjer.Data;
using Fryzjer.Models;
using Microsoft.AspNetCore.Identity;
using Fryzjer.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;

namespace Fryzjer.Pages.Admin
{
    public class AddEmployeeModel : PageModel
    {
        private HairdresserRepository _hairdresserRepository;
        private PlaceRepository _placeRepository;

        private readonly FryzjerContext _context;

        public AddEmployeeModel(FryzjerContext context)
        {
            _hairdresserRepository = new HairdresserRepository(context);
            _placeRepository = new PlaceRepository(context);
            _context = context;
        }

        [BindProperty]
        public Hairdresser NewHairdresser { get; set; } = new Hairdresser();

        public List<Place> Places { get; set; } = new List<Place>();

        public void OnGet()
        {
            Places = _placeRepository.getAll();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                Places = _placeRepository.getAll();
                return Page();
            }

            if (_context.Hairdresser.Any(h => h.login == NewHairdresser.login))
            {
                Places = _placeRepository.getAll();
                ModelState.AddModelError("NewHairdresser.login", "Ten login jest ju¿ zajêty.");
                return Page();
            }
            if (_context.Client.Any(c => c.Login == NewHairdresser.login))
            {
                Places = _placeRepository.getAll();
                ModelState.AddModelError("NewHairdresser.login", "Ten login jest ju¿ zajêty.");
                return Page();
            }
            if (_context.Administrator.Any(a => a.Login == NewHairdresser.login))
            {
                Places = _placeRepository.getAll();
                ModelState.AddModelError("NewHairdresser.login", "Ten login jest ju¿ zajêty.");
                return Page();
            }

            if (string.IsNullOrWhiteSpace(NewHairdresser.password))
            {
                ModelState.AddModelError("NewHairdresser.password", "Has³o jest wymagane podczas rejestracji.");
                return Page();
            }

            try
            {
                var hasher = new PasswordHasher<string>();
                NewHairdresser.password = hasher.HashPassword(null, NewHairdresser.password);

                _hairdresserRepository.insert(NewHairdresser);
                _hairdresserRepository.save();
                return RedirectToPage("/Admin/Employee/EmployeeManagement");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Wyst¹pi³ b³¹d podczas zapisywania pracownika: " + ex.Message);
                Places = _placeRepository.getAll();
                return Page();
            }
        }
    }
}