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
            // Pobierz wszystkie us�ugi
            Services = _serviceRepository.getAll();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            // Znajd� us�ug� po id
            var service = _serviceRepository.getById(id);

            if (service == null)
            {
                // Je�li nie znaleziono us�ugi, wr�� do listy us�ug
                return NotFound();
            }

            // Usu� us�ug�
            _serviceRepository.deleteById(service.Id);
            _serviceRepository.save();

            // Po usuni�ciu przekieruj u�ytkownika na stron� listy us�ug
            return RedirectToPage();
        }
    }
}
