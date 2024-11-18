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

            // SprawdŸ, czy login ju¿ istnieje
            if (_context.Client.Any(c => c.Login == Client.Login))
            {
                ErrorMessage = "Login zajêty, spróbuj inny.";
                return Page();
            }

            // Haszowanie has³a
            var hasher = new PasswordHasher<string>();
            Client.Password = hasher.HashPassword(null, Client.Password);

            try
            {
                _context.Client.Add(Client);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                ErrorMessage = "Wyst¹pi³ b³¹d podczas zapisywania danych: " + ex.Message;
                return Page();
            }

            // Ustaw komunikat i przekieruj na stronê logowania
            TempData["SuccessMessage"] = "Konto zosta³o dodane.";
            return RedirectToPage("/Login");
        }
    }
}
