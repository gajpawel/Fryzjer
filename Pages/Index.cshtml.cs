using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Fryzjer.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {
            // Mo¿esz tu wykorzystaæ logger, jeœli potrzeba
            _logger.LogInformation("Odwiedzono stronê g³ówn¹.");
        }

        public IActionResult OnPostLogout()
        {
            HttpContext.Session.Clear(); // Wyczyœæ sesjê
            _logger.LogInformation("U¿ytkownik zosta³ wylogowany."); // Informacja w logach
            return RedirectToPage("/Index"); // Przekierowanie na stronê g³ówn¹
        }
    }
}
