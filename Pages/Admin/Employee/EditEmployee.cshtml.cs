using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Fryzjer.Data;
using Fryzjer.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Fryzjer.Repositories;

namespace Fryzjer.Pages.Admin
{
    public class EditEmployeeModel : PageModel
    {
        private HairdresserRepository _hairdresserRepository;
        private PlaceRepository _placeRepository;

        private readonly FryzjerContext _context;
        public EditEmployeeModel(FryzjerContext context)
        {
            _hairdresserRepository = new HairdresserRepository(context);
            _placeRepository = new PlaceRepository(context);
            _context = context;
        }

        [BindProperty]
        public Hairdresser Hairdresser { get; set; }
        public List<Place> Places { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Hairdresser = _hairdresserRepository.getById(id);

            if (Hairdresser == null)
            {
                return NotFound();
            }

            Places = _placeRepository.getAll();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                Places = _placeRepository.getAll();
                return Page();
            }

            var existingHairdresser = _hairdresserRepository.getById(Hairdresser.Id);

            if (existingHairdresser == null)
            {
                return NotFound();
            }
            
            //Sprawdzenie czy login nie jest zajêty
            if(existingHairdresser.login != Hairdresser.login && (_context.Client.Any(c => c.Login == Hairdresser.login) || _context.Hairdresser.Any(h => h.login == Hairdresser.login) || _context.Administrator.Any(c => c.Login == Hairdresser.login))) {
                Places = _placeRepository.getAll();
                ModelState.AddModelError("Hairdresser.login", "Ten login jest ju¿ zajêty.");
                return Page();
            }

            // Aktualizuj tylko zmienione pola
            existingHairdresser.Name = Hairdresser.Name;
            existingHairdresser.Surname = Hairdresser.Surname;
            existingHairdresser.login = Hairdresser.login;
            existingHairdresser.PlaceId = Hairdresser.PlaceId;

            // Aktualizuj has³o tylko jeœli zosta³o podane nowe
            if (!string.IsNullOrEmpty(Hairdresser.password))
            {
                var hasher = new PasswordHasher<string>();
                existingHairdresser.password = hasher.HashPassword(null, Hairdresser.password);
            }

            try
            {
                _hairdresserRepository.save();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_hairdresserRepository.exists(Hairdresser.Id))
                {
                    return NotFound();
                }
                throw;
            }

            return RedirectToPage("/Admin/Employee/EmployeeManagement");
        }
    }

}
