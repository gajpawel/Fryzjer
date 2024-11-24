using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Fryzjer.Data;
using Fryzjer.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fryzjer.Pages.Admin
{
    public class EditEmployeeModel : PageModel
    {
        private readonly FryzjerContext _context;
        public EditEmployeeModel(FryzjerContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Hairdresser Hairdresser { get; set; }
        public List<Place> Places { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Hairdresser = await _context.Hairdresser
                .FirstOrDefaultAsync(h => h.Id == id);

            if (Hairdresser == null)
            {
                return NotFound();
            }

            Places = await _context.Place.ToListAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                Places = await _context.Place.ToListAsync();
                return Page();
            }

            var existingHairdresser = await _context.Hairdresser
                .FirstOrDefaultAsync(h => h.Id == Hairdresser.Id);

            if (existingHairdresser == null)
            {
                return NotFound();
            }

            // Aktualizuj tylko zmienione pola
            existingHairdresser.Name = Hairdresser.Name;
            existingHairdresser.Surname = Hairdresser.Surname;
            existingHairdresser.login = Hairdresser.login;
            existingHairdresser.PlaceId = Hairdresser.PlaceId;

            // Aktualizuj has³o tylko jeœli zosta³o podane nowe
            if (!string.IsNullOrEmpty(Hairdresser.password))
            {
                existingHairdresser.password = Hairdresser.password;
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!HairdresserExists(Hairdresser.Id))
                {
                    return NotFound();
                }
                throw;
            }

            return RedirectToPage("/Admin/EmployeeManagement");
        }

        private bool HairdresserExists(int id)
        {
            return _context.Hairdresser.Any(h => h.Id == id);
        }
    }

}
