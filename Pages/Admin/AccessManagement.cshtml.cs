using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Fryzjer.Data;
using Fryzjer.Models;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;

namespace Fryzjer.Pages.Admin
{
    public class AccessManagementModel : PageModel //zarz�dzanie sposobem logowania na konto admina
    {
        private readonly FryzjerContext _context;
        public string UserName { get; private set; } = string.Empty;
        public string SuccessMessage { get; private set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;

        public AccessManagementModel(FryzjerContext context)
        {
            _context = context;
        }


        public void OnGet()
        {
            LoadUserData();
        }

        private void LoadUserData()
        {
            // Pobierz dane u�ytkownika z sesji
            var userLogin = HttpContext.Session.GetString("UserLogin");
            if (!string.IsNullOrEmpty(userLogin))
            {
                var user = _context.Administrator.FirstOrDefault(c => c.Login == userLogin);
                if (user != null)
                {
                    UserName = user.Login ?? "Nieznany u�ytkownik";
                }
            }
        }

        private List<string> ValidatePassword(string password)
        {
            var results = new List<string>();

            // Walidacja z modelu Client
            var admin = new Administrator { Password = password };
            var validationContext = new ValidationContext(admin) { MemberName = nameof(Administrator.Password) };
            var validationResults = new List<ValidationResult>();

            bool isValid = Validator.TryValidateProperty(admin.Password, validationContext, validationResults);
            if (!isValid)
            {
                foreach (var validationResult in validationResults)
                {
                    results.Add(validationResult.ErrorMessage);
                }
            }

            return results;
        }

        public IActionResult OnPostChangePassword(string CurrentPassword, string NewPassword, string NewLogin)
        {
            // Pobierz login z sesji
            var userLogin = HttpContext.Session.GetString("UserLogin");
            if (string.IsNullOrEmpty(userLogin))
            {
                ModelState.AddModelError(string.Empty, "Wyst�pi� b��d. Spr�buj ponownie.");
                LoadUserData(); // Za�aduj dane u�ytkownika
                return RedirectToPage("/Admin/AdminProfile");
            }

            // Pobierz u�ytkownika z bazy
            var user = _context.Administrator.FirstOrDefault(c => c.Login == userLogin);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "U�ytkownik nie zosta� znaleziony.");
                LoadUserData(); // Za�aduj dane u�ytkownika
                return RedirectToPage("/Admin/AdminProfile");
            }

            if (NewLogin != null) 
            {
                if (_context.Client.Any(c => c.Login == NewLogin))
                {
                    ModelState.AddModelError("Administrator.Login", "Ten login jest ju� zaj�ty.");
                    return Page();
                }
                if (_context.Hairdresser.Any(c => c.login == NewLogin))
                {
                    ModelState.AddModelError("Administrator.Login", "Ten login jest ju� zaj�ty.");
                    return Page();
                }
                if (_context.Administrator.Any(c => c.Login == NewLogin))
                {
                    ModelState.AddModelError("Administrator.Login", "Ten login jest ju� zaj�ty.");
                    return Page();
                }
            }

            // Zweryfikuj obecne has�o
            var hasher = new PasswordHasher<string>();
            var verificationResult = hasher.VerifyHashedPassword(null, user.Password, CurrentPassword);
            if (verificationResult != PasswordVerificationResult.Success)
            {
                ModelState.AddModelError(string.Empty, "Nieprawid�owe obecne has�o.");
                LoadUserData(); // Za�aduj dane u�ytkownika
                return Page();
            }

            // Walidacja nowego has�a
            var passwordValidationResults = ValidatePassword(NewPassword);
            if (passwordValidationResults.Count > 0)
            {
                foreach (var error in passwordValidationResults)
                {
                    ModelState.AddModelError(string.Empty, error);
                }
                LoadUserData(); // Za�aduj dane u�ytkownika
                return Page();
            }

            // Zmie� has�o w bazie danych
            user.Password = hasher.HashPassword(null, NewPassword);
            if (NewLogin != null)
                user.Login = NewLogin;
            _context.SaveChanges();

            // Ustaw komunikat o sukcesie
            SuccessMessage = "Has�o zosta�o zmienione.";

            // Od�wie� dane u�ytkownika
            LoadUserData();
            return RedirectToPage("/Admin/AdminProfile"); // Pozosta� na tej samej stronie
        }
    }
}