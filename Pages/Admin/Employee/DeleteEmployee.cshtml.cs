using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Fryzjer.Models;
using Fryzjer.Data;

namespace Fryzjer.Pages.Admin
{
    public class DeleteEmployeeModel : PageModel
    {
        private readonly FryzjerContext _context;

        public DeleteEmployeeModel(FryzjerContext context)
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

            Hairdresser = await _context.Hairdresser.FindAsync(id);

            if (Hairdresser == null)
            {
                return NotFound();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var hairdresser = await _context.Hairdresser.FindAsync(id);

            if (hairdresser != null)
            {
                _context.Hairdresser.Remove(hairdresser);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("/Admin/Employee/EmployeeManagement");
        }
    }
}
