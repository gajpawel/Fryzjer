using Microsoft.AspNetCore.Mvc.RazorPages;
using Fryzjer.Models;
using Fryzjer.Data;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fryzjer.Repositories;

namespace Fryzjer.Pages.Admin
{
    public class SalonModel : PageModel
    {
        private PlaceRepository _placeRepository;

        // Lista salonów
        public List<Place> Places { get; set; }

        public SalonModel(FryzjerContext context)
        {
            _placeRepository = new PlaceRepository(context);
        }

        // Metoda GET - ³adowanie listy salonów
        public void OnGet()
        {
            // Pobieramy wszystkie salony, w tym nowe pola photoPath i description
            Places = _placeRepository.getAll();
        }

        // Metoda POST - usuwanie salonu
        public async Task<IActionResult> OnPostAsync(int id)
        {
            // Szukamy salonu po id
            var place = _placeRepository.getById(id);

            if (place != null)
            {
                // Usuwamy salon z bazy
                _placeRepository.deleteById(place.Id);
                _placeRepository.save();
            }

            // Po usuniêciu przekierowujemy na stronê z list¹ salonów
            return RedirectToPage("/Admin/Salon/Salon");
        }
    }
}
