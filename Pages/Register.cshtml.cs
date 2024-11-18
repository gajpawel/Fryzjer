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

            // Sprawd�, czy login ju� istnieje
            if (_context.Client.Any(c => c.Login == Client.Login))
            {
                ErrorMessage = "Login zaj�ty, spr�buj inny.";
                return Page();
            }

            // Haszowanie has�a
            var hasher = new PasswordHasher<string>();
            Client.Password = hasher.HashPassword(null, Client.Password);

            try
            {
                _context.Client.Add(Client);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                ErrorMessage = "Wyst�pi� b��d podczas zapisywania danych: " + ex.Message;
                return Page();
            }

            // Ustaw komunikat i przekieruj na stron� logowania
            TempData["SuccessMessage"] = "Konto zosta�o dodane.";
            return RedirectToPage("/Login");
        }
    }
}
