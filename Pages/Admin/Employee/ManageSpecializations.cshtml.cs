using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Fryzjer.Data;
using Fryzjer.Models;

namespace Fryzjer.Pages.Admin
{
    public class ManageSpecializationsModel : PageModel
    {
        private readonly FryzjerContext _context;

        public ManageSpecializationsModel(FryzjerContext context)
        {
            _context = context;
        }

        [BindProperty]
        public int HairdresserId { get; set; }

        public Hairdresser? Hairdresser { get; set; }
        public List<Service> AvailableServices { get; set; } = new();
        public List<Specialization> CurrentSpecializations { get; set; } = new();
        [BindProperty]
        public List<int> SelectedServices { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int hairdresserId)
        {
            HairdresserId = hairdresserId;

            Hairdresser = await _context.Hairdresser
                .FirstOrDefaultAsync(h => h.Id == hairdresserId);

            if (Hairdresser == null)
            {
                return NotFound();
            }

            AvailableServices = await _context.Service.ToListAsync();
            CurrentSpecializations = await _context.Specialization
                .Where(s => s.HairdresserId == hairdresserId)
                .ToListAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Usuñ wszystkie obecne specjalizacje
            var currentSpecs = await _context.Specialization
                .Where(s => s.HairdresserId == HairdresserId)
                .ToListAsync();
            _context.Specialization.RemoveRange(currentSpecs);

            // Dodaj nowe specjalizacje
            foreach (var serviceId in SelectedServices)
            {
                _context.Specialization.Add(new Specialization
                {
                    HairdresserId = HairdresserId,
                    ServiceId = serviceId
                });
            }

            await _context.SaveChangesAsync();

            return RedirectToPage("/Admin/Employee/EmployeeManagement");
        }
    }
}