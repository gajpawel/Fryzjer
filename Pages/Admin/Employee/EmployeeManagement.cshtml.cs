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
    public class EmployeeManagementModel : PageModel
    {
        private readonly FryzjerContext _context;

        public EmployeeManagementModel(FryzjerContext context)
        {
            _context = context;
        }

        // Property to hold the list of hairdressers
        public List<Hairdresser> Hairdressers { get; set; }

        // OnGet method to fetch the hairdressers from the database
        public async Task<IActionResult> OnGetAsync()
        {
            // Fetch hairdressers from the database or initialize an empty list
            Hairdressers = await _context.Hairdresser.ToListAsync();

            // If Hairdressers is null (although unlikely), initialize an empty list
            if (Hairdressers == null)
            {
                Hairdressers = new List<Hairdresser>();
            }

            return Page();
        }

        // Optional: You can add other methods for Add, Edit, or Delete operations
    }
}
