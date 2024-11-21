using Fryzjer.Data;
using Fryzjer.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Fryzjer.Pages
{
    public class LoginModel : PageModel
    {
        private readonly FryzjerContext _context;

        [BindProperty]
        public string Login { get; set; } = string.Empty;

        [BindProperty]
        public string Password { get; set; } = string.Empty;

        public string ErrorMessage { get; set; } = string.Empty;

        public LoginModel(FryzjerContext context)
        {
            _context = context;
        }

        public void OnGet()
        {
            if (TempData["SuccessMessage"] != null)
            {
                ViewData["SuccessMessage"] = TempData["SuccessMessage"].ToString();
            }

            Login = Request.Query["login"];
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                ErrorMessage = "Wprowadzone dane s¹ nieprawid³owe.";
                return Page();
            }

            // Logowanie jako admin
            if (Login == "Admin" && Password == "Admin")
            {
                HttpContext.Session.SetString("UserLogin", "Admin");
                return RedirectToPage("/Admin/AdminProfile"); // Przekierowanie na stronê admina
            }

            // Sprawdzenie w tabeli Fryzjerów (Hairdresser)
            var hairdresser = _context.Hairdresser.FirstOrDefault(h => h.login == Login);

            if (hairdresser != null)
            {
                // Weryfikacja has³a fryzjera
                var hasher = new PasswordHasher<string>();
                var result = hasher.VerifyHashedPassword(null, hairdresser.password, Password);

                if (result == PasswordVerificationResult.Success)
                {
                    HttpContext.Session.SetString("UserLogin", hairdresser.login);
                    return RedirectToPage("/Hairdressers/HairdresserMainPage"); // Przekierowanie na stronê g³ówn¹ fryzjera
                }
                else
                {
                    ErrorMessage = "Nieprawid³owy login lub has³o.";
                    return Page();
                }
            }

            // Sprawdzenie w tabeli Klientów (Client)
            var client = _context.Client.FirstOrDefault(c => c.Login == Login);

            if (client != null)
            {
                // Weryfikacja has³a klienta
                var hasher = new PasswordHasher<string>();
                var result = hasher.VerifyHashedPassword(null, client.Password, Password);

                if (result == PasswordVerificationResult.Success)
                {
                    HttpContext.Session.SetString("UserLogin", client.Login);
                    return RedirectToPage("/Index"); // Przekierowanie na stronê g³ówn¹ klienta
                }
                else
                {
                    ErrorMessage = "Nieprawid³owy login lub has³o.";
                    return Page();
                }
            }

            // Je¿eli login nie istnieje w ¿adnej tabeli
            ErrorMessage = "Nie znaleziono u¿ytkownika z takim loginem.";
            return Page();
        }
    }
}
