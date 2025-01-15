using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Fryzjer.Data;
using Fryzjer.Models;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Fryzjer.Repositories;

namespace Fryzjer.Pages.Admin
{
    public class ServicesModel : PageModel
    {
        private ServiceRepository _serviceRepository;

        [BindProperty]
        public Service NewService { get; set; }

        public IList<Service> Services { get; set; }

        public ServicesModel(FryzjerContext context)
        {
            _serviceRepository = new ServiceRepository(context);
        }

        public async Task OnGetAsync()
        {
            // Pobierz wszystkie us³ugi
            Services = _serviceRepository.getAll();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            // ZnajdŸ us³ugê po id
            var service = _serviceRepository.getById(id);

            if (service == null)
            {
                // Jeœli nie znaleziono us³ugi, wróæ do listy us³ug
                return NotFound();
            }

            // Usuñ us³ugê
            _serviceRepository.deleteById(service.Id);
            _serviceRepository.save();

            // Po usuniêciu przekieruj u¿ytkownika na stronê listy us³ug
            return RedirectToPage();
        }
    }
}
