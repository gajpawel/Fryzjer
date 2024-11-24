using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Fryzjer.Data;
using Fryzjer.Models;

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
        public Hairdresser NewHairdresser { get; set; } = new Hairdresser();

        public List<Place> Places { get; set; } = new List<Place>();

        public void OnGet()
        {
            Places = _context.Place.ToList();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                Places = _context.Place.ToList();
                return Page();
            }

            try
            {
                _context.Hairdresser.Add(NewHairdresser);
                await _context.SaveChangesAsync();
                return RedirectToPage("/Admin/EmployeeManagement");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Wyst¹pi³ b³¹d podczas zapisywania pracownika: " + ex.Message);
                Places = _context.Place.ToList();
                return Page();
            }
        }
    }
}