using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Fryzjer.Models;
using Fryzjer.Data;
using Fryzjer.Repositories;

namespace Fryzjer.Pages.Admin
{
    public class DeleteEmployeeModel : PageModel
    {
        private HairdresserRepository _hairdresserRepository;

        public DeleteEmployeeModel(FryzjerContext context)
        {
            _hairdresserRepository = new HairdresserRepository(context);
        }

        public Hairdresser Hairdresser { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Hairdresser = _hairdresserRepository.getById(id.GetValueOrDefault());

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

            var hairdresser = _hairdresserRepository.getById(id.GetValueOrDefault());

            if (hairdresser != null)
            {
                _hairdresserRepository.deleteById(hairdresser.Id);
                _hairdresserRepository.save();
            }

            return RedirectToPage("/Admin/Employee/EmployeeManagement");
        }
    }
}
