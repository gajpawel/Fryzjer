using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Fryzjer.Models;
using Fryzjer.Data;

namespace Fryzjer.Pages.Admin
{
    public class DetailsEmployeeModel : PageModel
    {
        private readonly FryzjerContext _context;

        public DetailsEmployeeModel(FryzjerContext context)
        {
            _context = context;
        }

        public Hairdresser Hairdresser { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Hairdresser = await _context.Hairdresser
                .Include(h => h.Place) // Jeœli chcesz równie¿ dane lokalu
                .FirstOrDefaultAsync(m => m.Id == id);

            if (Hairdresser == null)
            {
                return NotFound();
            }

            return Page();
        }
    }
}
