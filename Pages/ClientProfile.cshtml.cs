using Fryzjer.Data;
using Fryzjer.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Fryzjer.Pages
{
    public class ClientProfileModel : PageModel
    {
        private readonly FryzjerContext _context;

        public string UserName { get; private set; } = string.Empty;

        public string ActiveTab { get; private set; } = "reservations"; // Domy�lna aktywna zak�adka

        public string SuccessMessage { get; private set; } = string.Empty;

        public ClientProfileModel(FryzjerContext context)
        {
            _context = context;
        }

        public void OnGet()
        {
            LoadUserData();
        }

        public IActionResult OnPostChangePassword(string CurrentPassword, string NewPassword)
        {
            ActiveTab = "settings"; // Ustaw aktywn� zak�adk� na "Ustawienia"

            // Pobierz login z sesji
            var userLogin = HttpContext.Session.GetString("UserLogin");
            if (string.IsNullOrEmpty(userLogin))
            {
                ModelState.AddModelError(string.Empty, "Wyst�pi� b��d. Spr�buj ponownie.");
                return RedirectToPage("/Login");
            }

            // Pobierz u�ytkownika z bazy
            var user = _context.Client.FirstOrDefault(c => c.Login == userLogin);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "U�ytkownik nie zosta� znaleziony.");
                return RedirectToPage("/Login");
            }

            // Zweryfikuj obecne has�o
            var hasher = new PasswordHasher<string>();
            var verificationResult = hasher.VerifyHashedPassword(null, user.Password, CurrentPassword);
            if (verificationResult != PasswordVerificationResult.Success)
            {
                ModelState.AddModelError(string.Empty, "Nieprawid�owe obecne has�o.");
                return Page();
            }

            // Zmie� has�o w bazie danych
            user.Password = hasher.HashPassword(null, NewPassword);
            _context.SaveChanges();

            // Ustaw komunikat o sukcesie
            SuccessMessage = "Has�o zosta�o zmienione.";

            // Od�wie� dane u�ytkownika
            LoadUserData();

            return Page(); // Pozosta� na tej samej stronie
        }

        public IActionResult OnPostDeleteProfile()
        {
            ActiveTab = "settings"; // Ustaw aktywn� zak�adk� na "Ustawienia"

            // Pobierz login z sesji
            var userLogin = HttpContext.Session.GetString("UserLogin");
            if (string.IsNullOrEmpty(userLogin))
            {
                ModelState.AddModelError(string.Empty, "Wyst�pi� b��d. Spr�buj ponownie.");
                return RedirectToPage("/Login");
            }

            // Pobierz u�ytkownika z bazy
            var user = _context.Client.FirstOrDefault(c => c.Login == userLogin);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "U�ytkownik nie zosta� znaleziony.");
                return RedirectToPage("/Login");
            }

            // Usu� u�ytkownika z bazy
            _context.Client.Remove(user);
            _context.SaveChanges();

            // Wyloguj u�ytkownika
            HttpContext.Session.Remove("UserLogin");

            // Ustaw komunikat w TempData
            TempData["SuccessMessage"] = "Twoje konto zosta�o usuni�te.";
            return RedirectToPage("/Index");
        }

        private void LoadUserData()
        {
            // Pobierz dane u�ytkownika z sesji
            var userLogin = HttpContext.Session.GetString("UserLogin");
            if (!string.IsNullOrEmpty(userLogin))
            {
                var user = _context.Client.FirstOrDefault(c => c.Login == userLogin);
                if (user != null)
                {
                    UserName = user.Name ?? "Nieznany u�ytkownik";
                }
            }
        }
    }
}
