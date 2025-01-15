using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Fryzjer.Models;
using Fryzjer.Data;
using Fryzjer.Repositories;

namespace Fryzjer.Pages.Admin
{
    public class DetailsEmployeeModel : PageModel
    {
        private HairdresserRepository _hairdresserRepository;

        public DetailsEmployeeModel(FryzjerContext context)
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

            Hairdresser = _hairdresserRepository.getAndIncludePlace(id.GetValueOrDefault());

            if (Hairdresser == null)
            {
                return NotFound();
            }

            return Page();
        }
    }
}
