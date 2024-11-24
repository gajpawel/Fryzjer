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
            // Pobierz wszystkie us³ugi
            Services = await _context.Service.ToListAsync();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            // ZnajdŸ us³ugê po id
            var service = await _context.Service.FindAsync(id);

            if (service == null)
            {
                // Jeœli nie znaleziono us³ugi, wróæ do listy us³ug
                return NotFound();
            }

            // Usuñ us³ugê
            _context.Service.Remove(service);
            await _context.SaveChangesAsync();

            // Po usuniêciu przekieruj u¿ytkownika na stronê listy us³ug
            return RedirectToPage();
        }
    }
}
