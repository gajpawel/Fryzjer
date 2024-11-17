using Fryzjer.Data;
using Fryzjer.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Fryzjer.Pages
{
    public class RegisterModel : PageModel
    {
        private readonly FryzjerContext _context;

        public string ErrorMessage { get; set; } = string.Empty;

        [BindProperty]
        public string Name { get; set; } = string.Empty;

        [BindProperty]
        public string Surname { get; set; } = string.Empty;

        [BindProperty]
        public char Gender { get; set; } = 'M';

        [BindProperty]
        public string Login { get; set; } = string.Empty;

        [BindProperty]
        public string Password { get; set; } = string.Empty;

        [BindProperty]
        public string Phone { get; set; } = string.Empty;

        public RegisterModel(FryzjerContext context)
        {
            _context = context;
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                ErrorMessage = "Wprowadzone dane s� nieprawid�owe.";
                return Page();
            }

            // Sprawd�, czy login ju� istnieje
            if (_context.Client.Any(c => c.Login == Login))
            {
                ErrorMessage = "Login zaj�ty, spr�buj inny.";
                return Page();
            }

            // Dodaj nowego u�ytkownika
            var client = new Client
            {
                Name = Name,
                Surname = Surname,
                Gender = Gender,
                Login = Login,
                Password = Password,
                Phone = Phone
            };

            try
            {
                _context.Client.Add(client);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                ErrorMessage = "Wyst�pi� b��d podczas zapisywania danych: " + ex.Message;
                return Page();
            }

            // Ustaw komunikat w TempData i przekieruj na stron� logowania
            TempData["SuccessMessage"] = "Konto zosta�o dodane.";
            return RedirectToPage("/Login");
        }
    }
}
