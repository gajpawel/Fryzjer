using Fryzjer.Data;
using Fryzjer.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Fryzjer.Pages
{
    public class ViewSalonModel : PageModel
    {
        private readonly FryzjerContext _context;

        public ViewSalonModel(FryzjerContext context)
        {
            _context = context;
        }

        [BindProperty]
        public int SalonId { get; set; }

        public Place? Place { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            SalonId = id;

            Place = await _context.Place
                .FirstOrDefaultAsync(h => h.Id == id);

            if (Place == null)
            {
                return NotFound();
            }
            return Page();
        }
    }
}
