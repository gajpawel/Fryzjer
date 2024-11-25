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
            // Obs�uga wiadomo�ci po rejestracji
            if (TempData["SuccessMessage"] != null)
            {
                ViewData["SuccessMessage"] = TempData["SuccessMessage"].ToString();
            }

            // Przechwycenie loginu przekazanego w query string (np. po rejestracji)
            Login = Request.Query["login"];
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                ErrorMessage = "Wprowadzone dane s� nieprawid�owe.";
                return Page();
            }

            // Logowanie jako admin
            var administrator = _context.Administrator.FirstOrDefault(h => h.Login == Login);

            if (administrator != null)
            {
                var hasher = new PasswordHasher<string>();
                var result = hasher.VerifyHashedPassword(null, administrator.Password, Password);
                if (result == PasswordVerificationResult.Success) // Upewnij si�, �e has�a s� w odpowiednim formacie (np. zahaszowane)
                {
                    // Zapisanie danych fryzjera w sesji
                    HttpContext.Session.SetString("UserLogin", administrator.Login);
                    HttpContext.Session.SetInt32("HairdresserId", administrator.Id); // Kluczowe dla przekierowania na profil
                    HttpContext.Session.SetString("UserType", "Admin");
                    return RedirectToPage("/Admin/AdminProfile"); // Przekierowanie na stron� admina
                }
                else
                {
                    ErrorMessage = "Nieprawid�owy login lub has�o.";
                    return Page();
                }
            }

            // Logowanie fryzjera
            var hairdresser = _context.Hairdresser.FirstOrDefault(h => h.login == Login);

            if (hairdresser != null)
            {
                if (hairdresser.password == Password) // Upewnij si�, �e has�a s� w odpowiednim formacie (np. zahaszowane)
                {
                    // Zapisanie danych fryzjera w sesji
                    HttpContext.Session.SetString("UserLogin", hairdresser.login);
                    HttpContext.Session.SetInt32("HairdresserId", hairdresser.Id); // Kluczowe dla przekierowania na profil
                    HttpContext.Session.SetString("UserType", "Hairdresser");

                    return RedirectToPage("/Hairdressers/HairdresserMainPage"); // Przekierowanie na panel fryzjera
                }
                else
                {
                    ErrorMessage = "Nieprawid�owy login lub has�o.";
                    return Page();
                }
            }

            // Logowanie klienta
            var client = _context.Client.FirstOrDefault(c => c.Login == Login);

            if (client != null)
            {
                var hasher = new PasswordHasher<string>();
                var result = hasher.VerifyHashedPassword(null, client.Password, Password);

                if (result == PasswordVerificationResult.Success)
                {
                    // Zapisanie danych klienta w sesji
                    HttpContext.Session.SetString("UserLogin", client.Login);
                    HttpContext.Session.SetString("UserType", "Client");

                    return RedirectToPage("/Index"); // Przekierowanie na stron� g��wn� klienta
                }
                else
                {
                    ErrorMessage = "Nieprawid�owy login lub has�o.";
                    return Page();
                }
            }

            // Gdy login nie zosta� znaleziony
            ErrorMessage = "Nie znaleziono u�ytkownika z takim loginem.";
            return Page();
        }
    }
}
