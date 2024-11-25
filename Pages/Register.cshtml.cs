using Fryzjer.Data;
using Fryzjer.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Identity;

namespace Fryzjer.Pages
{
    public class RegisterModel : PageModel
    {
        private readonly FryzjerContext _context;

        public string ErrorMessage { get; set; } = string.Empty;

        [BindProperty]
        public Client Client { get; set; } = new Client();

        public RegisterModel(FryzjerContext context)
        {
            _context = context;
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Walidacja modelu
            if (!ModelState.IsValid)
            {
                ErrorMessage = "Wprowadzone dane s� nieprawid�owe.";
                return Page();
            }

            // Walidacja loginu i has�a
            if (string.IsNullOrWhiteSpace(Client.Login))
            {
                ModelState.AddModelError("Client.Login", "Login jest wymagany podczas rejestracji.");
                return Page();
            }

            if (string.IsNullOrWhiteSpace(Client.Password))
            {
                ModelState.AddModelError("Client.Password", "Has�o jest wymagane podczas rejestracji.");
                return Page();
            }

            // Sprawd�, czy login ju� istnieje
            if (_context.Client.Any(c => c.Login == Client.Login))
            {
                ModelState.AddModelError("Client.Login", "Ten login jest ju� zaj�ty.");
                return Page();
            }
            if (_context.Hairdresser.Any(c => c.login == Client.Login))
            {
                ModelState.AddModelError("Client.Login", "Ten login jest ju� zaj�ty.");
                return Page();
            }
            if (_context.Administrator.Any(c => c.Login == Client.Login))
            {
                ModelState.AddModelError("Client.Login", "Ten login jest ju� zaj�ty.");
                return Page();
            }

            // Haszowanie has�a
            var hasher = new PasswordHasher<string>();
            Client.Password = hasher.HashPassword(null, Client.Password);

            try
            {
                // Dodanie klienta do bazy danych
                _context.Client.Add(Client);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                ErrorMessage = "Wyst�pi� b��d podczas zapisywania danych: " + ex.Message;
                return Page();
            }

            // Ustaw komunikat sukcesu i przekierowanie na stron� logowania
            TempData["SuccessMessage"] = "Rejestracja zako�czona sukcesem! Mo�esz si� zalogowa�.";
            return RedirectToPage("/Login", new { login = Client.Login });
        }
    }
}
