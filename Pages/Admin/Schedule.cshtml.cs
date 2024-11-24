using Fryzjer.Data;
using Fryzjer.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fryzjer.Pages.Admin
{
    public class ScheduleModel : PageModel
    {
        private readonly FryzjerContext _context;

        public ScheduleModel(FryzjerContext context)
        {
            _context = context;
        }

        [BindProperty(SupportsGet = true)]
        public string SearchQuery { get; set; } = string.Empty;  // Wyszukiwanie

        public IList<Hairdresser> Hairdressers { get; set; } = new List<Hairdresser>();  // Lista fryzjerów

        public async Task OnGetAsync()
        {
            // Bazowe zapytanie pobieraj¹ce fryzjerów wraz z miejscami
            IQueryable<Hairdresser> query = _context.Hairdresser.Include(h => h.Place);

            // Filtrowanie na podstawie SearchQuery
            if (!string.IsNullOrWhiteSpace(SearchQuery))
            {
                query = query.Where(h =>
                    h.Name.Contains(SearchQuery) ||    // Wyszukiwanie po imieniu
                    h.Surname.Contains(SearchQuery) || // Wyszukiwanie po nazwisku
                    (h.Place != null && h.Place.Name.Contains(SearchQuery)) // Wyszukiwanie po nazwie miejsca
                );
            }

            // Pobranie wyników do listy
            Hairdressers = await query.ToListAsync();
        }
    }
}
