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

        public string ActiveTab { get; private set; } = "reservations"; // Domyœlna aktywna zak³adka

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
            ActiveTab = "settings"; // Ustaw aktywn¹ zak³adkê na "Ustawienia"

            // Pobierz login z sesji
            var userLogin = HttpContext.Session.GetString("UserLogin");
            if (string.IsNullOrEmpty(userLogin))
            {
                ModelState.AddModelError(string.Empty, "Wyst¹pi³ b³¹d. Spróbuj ponownie.");
                return RedirectToPage("/Login");
            }

            // Pobierz u¿ytkownika z bazy
            var user = _context.Client.FirstOrDefault(c => c.Login == userLogin);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "U¿ytkownik nie zosta³ znaleziony.");
                return RedirectToPage("/Login");
            }

            // Zweryfikuj obecne has³o
            var hasher = new PasswordHasher<string>();
            var verificationResult = hasher.VerifyHashedPassword(null, user.Password, CurrentPassword);
            if (verificationResult != PasswordVerificationResult.Success)
            {
                ModelState.AddModelError(string.Empty, "Nieprawid³owe obecne has³o.");
                return Page();
            }

            // Zmieñ has³o w bazie danych
            user.Password = hasher.HashPassword(null, NewPassword);
            _context.SaveChanges();

            // Ustaw komunikat o sukcesie
            SuccessMessage = "Has³o zosta³o zmienione.";

            // Odœwie¿ dane u¿ytkownika
            LoadUserData();

            return Page(); // Pozostañ na tej samej stronie
        }

        public IActionResult OnPostDeleteProfile()
        {
            ActiveTab = "settings"; // Ustaw aktywn¹ zak³adkê na "Ustawienia"

            // Pobierz login z sesji
            var userLogin = HttpContext.Session.GetString("UserLogin");
            if (string.IsNullOrEmpty(userLogin))
            {
                ModelState.AddModelError(string.Empty, "Wyst¹pi³ b³¹d. Spróbuj ponownie.");
                return RedirectToPage("/Login");
            }

            // Pobierz u¿ytkownika z bazy
            var user = _context.Client.FirstOrDefault(c => c.Login == userLogin);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "U¿ytkownik nie zosta³ znaleziony.");
                return RedirectToPage("/Login");
            }

            // Usuñ u¿ytkownika z bazy
            _context.Client.Remove(user);
            _context.SaveChanges();

            // Wyloguj u¿ytkownika
            HttpContext.Session.Remove("UserLogin");

            // Ustaw komunikat w TempData
            TempData["SuccessMessage"] = "Twoje konto zosta³o usuniête.";
            return RedirectToPage("/Index");
        }

        private void LoadUserData()
        {
            // Pobierz dane u¿ytkownika z sesji
            var userLogin = HttpContext.Session.GetString("UserLogin");
            if (!string.IsNullOrEmpty(userLogin))
            {
                var user = _context.Client.FirstOrDefault(c => c.Login == userLogin);
                if (user != null)
                {
                    UserName = user.Name ?? "Nieznany u¿ytkownik";
                }
            }
        }
    }
}
