using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Fryzjer.Pages
{
    public class ProfileModel : PageModel
    {
        public string UserLogin { get; private set; } = string.Empty;

        public void OnGet()
        {
            // Pobierz login u¿ytkownika z sesji
            UserLogin = HttpContext.Session.GetString("UserLogin") ?? "Nieznany u¿ytkownik";
        }
    }
}

