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

        // OnGetAsync is for handling the request to edit a hairdresser
        public async Task<IActionResult> OnGetAsync(int id)
        {
            Hairdresser = await _context.Hairdresser
                .FirstOrDefaultAsync(h => h.Id == id);

            // If Hairdresser not found, return NotFound error
            if (Hairdresser == null)
            {
                return NotFound();
            }

            // Fetch all places for the dropdown
            Places = await _context.Place.ToListAsync();

            return Page();
        }

        // OnPost method to save the updated hairdresser data
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                // If validation fails, reload the page with the same data
                Places = await _context.Place.ToListAsync();
                return Page();
            }

            _context.Attach(Hairdresser).State = EntityState.Modified;

            try
            {
                // Save changes to the database
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!HairdresserExists(Hairdresser.Id))
                {
                    return NotFound();  // If the hairdresser is no longer available
                }
                else
                {
                    throw;
                }
            }

            return RedirectToPage("/Admin/EmployeeManagement");
        }

        private bool HairdresserExists(int id)
        {
            return _context.Hairdresser.Any(h => h.Id == id);
        }
    }
}
