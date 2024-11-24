using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Fryzjer.Data;
using Fryzjer.Models;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Fryzjer.Pages.Admin
{
    public class ServicesModel : PageModel
    {
        private readonly FryzjerContext _context;

        [BindProperty]
        public Service NewService { get; set; }

        public IList<Service> Services { get; set; }

        public ServicesModel(FryzjerContext context)
        {
            _context = context;
        }

        public async Task OnGetAsync()
        {
            // Pobierz wszystkie us�ugi
            Services = await _context.Service.ToListAsync();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            // Znajd� us�ug� po id
            var service = await _context.Service.FindAsync(id);

            if (service == null)
            {
                // Je�li nie znaleziono us�ugi, wr�� do listy us�ug
                return NotFound();
            }

            // Usu� us�ug�
            _context.Service.Remove(service);
            await _context.SaveChangesAsync();

            // Po usuni�ciu przekieruj u�ytkownika na stron� listy us�ug
            return RedirectToPage();
        }
    }
}
