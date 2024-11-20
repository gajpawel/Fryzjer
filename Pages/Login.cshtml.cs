using Fryzjer.Data;
using Fryzjer.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Reflection.Metadata;

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

            // SprawdŸ poprawnoœæ danych logowania z bazy danych
            var user = _context.Client.FirstOrDefault(c => c.Login == Login);

            if (user == null)
            {
                ErrorMessage = "Nieprawid³owy login lub has³o.";
                return Page();
            }

            // Weryfikacja has³a
            var hasher = new PasswordHasher<string>();
            var result = hasher.VerifyHashedPassword(null, user.Password, Password);

            if (result == PasswordVerificationResult.Success)
            {
                // Przechowaj login u¿ytkownika w sesji
                HttpContext.Session.SetString("UserLogin", user.Login);
                // Logowanie udane
                return RedirectToPage("/Index");
            }
            else
            {
                ErrorMessage = "Nieprawid³owy login lub has³o.";
                return Page();
            }
        }

    }




}

