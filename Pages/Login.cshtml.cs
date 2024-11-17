using Fryzjer.Data;
using Fryzjer.Models;
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
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                ErrorMessage = "Wprowadzone dane s� nieprawid�owe.";
                return Page();
            }

            // Sprawd� poprawno�� danych logowania
            var user = _context.Client.FirstOrDefault(c => c.Login == Login && c.Password == Password);

            if (user == null)
            {
                ErrorMessage = "Nieprawid�owy login lub has�o.";
                return Page();
            }

            return RedirectToPage("/Index");
        }
    }
}
