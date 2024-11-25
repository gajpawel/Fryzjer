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
                ErrorMessage = "Wprowadzone dane s¹ nieprawid³owe.";
                return Page();
            }

            // Walidacja loginu i has³a
            if (string.IsNullOrWhiteSpace(Client.Login))
            {
                ModelState.AddModelError("Client.Login", "Login jest wymagany podczas rejestracji.");
                return Page();
            }

            if (string.IsNullOrWhiteSpace(Client.Password))
            {
                ModelState.AddModelError("Client.Password", "Has³o jest wymagane podczas rejestracji.");
                return Page();
            }

            // SprawdŸ, czy login ju¿ istnieje
            if (_context.Client.Any(c => c.Login == Client.Login))
            {
                ModelState.AddModelError("Client.Login", "Ten login jest ju¿ zajêty.");
                return Page();
            }
            if (_context.Hairdresser.Any(c => c.login == Client.Login))
            {
                ModelState.AddModelError("Client.Login", "Ten login jest ju¿ zajêty.");
                return Page();
            }
            if (_context.Administrator.Any(c => c.Login == Client.Login))
            {
                ModelState.AddModelError("Client.Login", "Ten login jest ju¿ zajêty.");
                return Page();
            }

            // Haszowanie has³a
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
                ErrorMessage = "Wyst¹pi³ b³¹d podczas zapisywania danych: " + ex.Message;
                return Page();
            }

            // Ustaw komunikat sukcesu i przekierowanie na stronê logowania
            TempData["SuccessMessage"] = "Rejestracja zakoñczona sukcesem! Mo¿esz siê zalogowaæ.";
            return RedirectToPage("/Login", new { login = Client.Login });
        }
    }
}
