using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Fryzjer.Data;
using Fryzjer.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fryzjer.Pages.Admin
{
    public class AddEmployeeModel : PageModel
    {
        private readonly FryzjerContext _context;

        public AddEmployeeModel(FryzjerContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Hairdresser NewHairdresser { get; set; }

        // To display the list of places (assuming you have a 'Place' model)
        public List<Place> Places { get; set; }

        public void OnGet()
        {
            // Load all available places to populate the dropdown
            Places = _context.Place.ToList();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                // Jeœli PlaceId jest puste, ustaw wartoœæ na null
                if (string.IsNullOrEmpty(Request.Form["PlaceId"]))
                {
                    NewHairdresser.PlaceId = null;
                }

                _context.Hairdresser.Add(NewHairdresser);
                await _context.SaveChangesAsync();
                return RedirectToPage("/Admin/EmployeeManagement");
            }

            // If the model is not valid, redisplay the page
            Places = _context.Place.ToList(); // Re-load places to show in the dropdown
            return Page();
        }
    }
}
