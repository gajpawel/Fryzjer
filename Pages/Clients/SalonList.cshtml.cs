using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Fryzjer.Models;
using Fryzjer.Data;
using System.Linq;

namespace Fryzjer.Pages.Admin
{
    public class SalonListModel : PageModel
    {
        private readonly FryzjerContext _context;

        public SalonListModel(FryzjerContext context)
        {
            _context = context;
        }

        public IList<Place> Places { get; set; }

        // Metoda OnGet() do wczytania listy salonów
        public void OnGet()
        {
            Places = _context.Place.ToList(); // Pobieranie wszystkich salonów z bazy danych
        }
    }
}
