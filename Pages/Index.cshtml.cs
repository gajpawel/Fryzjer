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
            // Mo�esz tu wykorzysta� logger, je�li potrzeba
            _logger.LogInformation("Odwiedzono stron� g��wn�.");
        }

        public IActionResult OnPostLogout()
        {
            // Usu� sesj� u�ytkownika
            HttpContext.Session.Remove("UserLogin");
            _logger.LogInformation("U�ytkownik zosta� wylogowany."); // Informacja w logach
            HttpContext.Session.Remove("HairdresserId");
            HttpContext.Session.Remove("UserType");
            return RedirectToPage("/Index"); // Przekierowanie na stron� g��wn�
        }
    }
}