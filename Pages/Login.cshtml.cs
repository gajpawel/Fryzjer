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
            // Obs³uga wiadomoœci po rejestracji
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
                ErrorMessage = "Wprowadzone dane s¹ nieprawid³owe.";
                return Page();
            }

            // Logowanie jako admin
            var administrator = _context.Administrator.FirstOrDefault(h => h.Login == Login);

            if (administrator != null)
            {
                var hasher = new PasswordHasher<string>();
                var result = hasher.VerifyHashedPassword(null, administrator.Password, Password);
                if (result == PasswordVerificationResult.Success) // Upewnij siê, ¿e has³a s¹ w odpowiednim formacie (np. zahaszowane)
                {
                    // Zapisanie danych fryzjera w sesji
                    HttpContext.Session.SetString("UserLogin", administrator.Login);
                    HttpContext.Session.SetInt32("HairdresserId", administrator.Id); // Kluczowe dla przekierowania na profil
                    HttpContext.Session.SetString("UserType", "Admin");
                    return RedirectToPage("/Admin/AdminProfile"); // Przekierowanie na stronê admina
                }
                else
                {
                    ErrorMessage = "Nieprawid³owy login lub has³o.";
                    return Page();
                }
            }

            // Logowanie fryzjera
            var hairdresser = _context.Hairdresser.FirstOrDefault(h => h.login == Login);

            if (hairdresser != null)
            {
                if (hairdresser.password == Password) // Upewnij siê, ¿e has³a s¹ w odpowiednim formacie (np. zahaszowane)
                {
                    // Zapisanie danych fryzjera w sesji
                    HttpContext.Session.SetString("UserLogin", hairdresser.login);
                    HttpContext.Session.SetInt32("HairdresserId", hairdresser.Id); // Kluczowe dla przekierowania na profil
                    HttpContext.Session.SetString("UserType", "Hairdresser");

                    return RedirectToPage("/Hairdressers/HairdresserMainPage"); // Przekierowanie na panel fryzjera
                }
                else
                {
                    ErrorMessage = "Nieprawid³owy login lub has³o.";
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

                    return RedirectToPage("/Index"); // Przekierowanie na stronê g³ówn¹ klienta
                }
                else
                {
                    ErrorMessage = "Nieprawid³owy login lub has³o.";
                    return Page();
                }
            }

            // Gdy login nie zosta³ znaleziony
            ErrorMessage = "Nie znaleziono u¿ytkownika z takim loginem.";
            return Page();
        }
    }
}
