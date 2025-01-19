using Fryzjer.Data;
using Fryzjer.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Fryzjer.Pages.Clients
{
    public class ClientProfileModel : PageModel
    {
        private readonly FryzjerContext _context;

        public string UserName { get; private set; } = string.Empty;

        public string ActiveTab { get; private set; } = "reservations"; // Domyœlna aktywna zak³adka

        public string SuccessMessage { get; private set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
		public List<Reservation> CurrentReservations { get; set; } = new();
		public List<Reservation> PastReservations { get; set; } = new();
        public Reservation previousReservation { get; set; }


		public ClientProfileModel(FryzjerContext context)
        {
            _context = context;
        }


        public void OnGet()
		{
			LoadUserData();

			// Pobierz login klienta z sesji
			var userLogin = HttpContext.Session.GetString("UserLogin");
			if (string.IsNullOrEmpty(userLogin)) return;

			// Pobierz klienta z bazy danych
			var client = _context.Client.FirstOrDefault(c => c.Login == userLogin);
			if (client == null) return;

			// Pobierz rezerwacje
			var now = DateTime.Now.AddDays(-1);

            var reservationsToUpdate = _context.Reservation
                .Where(r => r.status != 'A' && r.date < now)
                .ToList();

            // Aktualizujemy status na 'Z'
            foreach (var reservation in reservationsToUpdate)
            {
                reservation.status = 'Z';
            }

            // Zapisujemy zmiany w bazie danych
            _context.SaveChanges();

            // Pobierz dane bez sortowania
            CurrentReservations = _context.Reservation
                .Include(r => r.Hairdresser)
                .Include(r => r.Service)
                .Where(r => r.ClientId == client.Id && r.date >= now.Date && (r.status == 'O' || r.status == 'P'))
                .AsEnumerable() // Prze³¹czenie na LINQ to Objects
                .OrderBy(r => r.date)
                .ThenBy(r => r.time)
                .ToList();

            PastReservations = _context.Reservation
                .Include(r => r.Hairdresser)
                .Include(r => r.Service)
                .Where(r => r.ClientId == client.Id && (r.date < now.Date || (r.status == 'Z' || r.status == 'A')))
                .AsEnumerable() // Prze³¹czenie na LINQ to Objects
                .OrderByDescending(r => r.date)
                .ThenByDescending(r => r.time)
                .ToList();
        }


        public IActionResult OnPostChangePassword(string CurrentPassword, string NewPassword)
        {
            ActiveTab = "settings"; // Ustaw aktywn¹ zak³adkê na "Ustawienia"

            // Pobierz login z sesji
            var userLogin = HttpContext.Session.GetString("UserLogin");
            if (string.IsNullOrEmpty(userLogin))
            {
                ModelState.AddModelError(string.Empty, "Wyst¹pi³ b³¹d. Spróbuj ponownie.");
                LoadUserData(); // Za³aduj dane u¿ytkownika
                return RedirectToPage("/Login");
            }

            // Pobierz u¿ytkownika z bazy
            var user = _context.Client.FirstOrDefault(c => c.Login == userLogin);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "U¿ytkownik nie zosta³ znaleziony.");
                LoadUserData(); // Za³aduj dane u¿ytkownika
                return RedirectToPage("/Login");
            }

            // Zweryfikuj obecne has³o
            var hasher = new PasswordHasher<string>();
            var verificationResult = hasher.VerifyHashedPassword(null, user.Password, CurrentPassword);
            if (verificationResult != PasswordVerificationResult.Success)
            {
                ModelState.AddModelError(string.Empty, "Nieprawid³owe obecne has³o.");
                LoadUserData(); // Za³aduj dane u¿ytkownika
                return Page();
            }

            // Walidacja nowego has³a
            var passwordValidationResults = ValidatePassword(NewPassword);
            if (passwordValidationResults.Count > 0)
            {
                foreach (var error in passwordValidationResults)
                {
                    ModelState.AddModelError(string.Empty, error);
                }
                LoadUserData(); // Za³aduj dane u¿ytkownika
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
                LoadUserData(); // Za³aduj dane u¿ytkownika
                return RedirectToPage("/Login");
            }

            // Pobierz u¿ytkownika z bazy
            var user = _context.Client.FirstOrDefault(c => c.Login == userLogin);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "U¿ytkownik nie zosta³ znaleziony.");
                LoadUserData(); // Za³aduj dane u¿ytkownika
                return RedirectToPage("/Login");
            }

            // Usuñ u¿ytkownika z bazy
            _context.Client.Remove(user);
            _context.SaveChanges();

            // Wyloguj u¿ytkownika
            HttpContext.Session.Remove("UserLogin");
            HttpContext.Session.Remove("UserType");

            // Ustaw komunikat w TempData (widoczny w Layout)
            TempData["AccountDeleted"] = "Twoje konto zosta³o usuniête.";
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

        private List<string> ValidatePassword(string password)
        {
            var results = new List<string>();

            // Walidacja z modelu Client
            var client = new Client { Password = password };
            var validationContext = new ValidationContext(client) { MemberName = nameof(Client.Password) };
            var validationResults = new List<ValidationResult>();

            bool isValid = Validator.TryValidateProperty(client.Password, validationContext, validationResults);
            if (!isValid)
            {
                foreach (var validationResult in validationResults)
                {
                    results.Add(validationResult.ErrorMessage);
                }
            }

            return results;
        }
        public IActionResult OnPostCancelReservation(int reservationId)
        {
            var reservation = _context.Reservation.FirstOrDefault(r => r.Id == reservationId);

            if (reservation != null && (reservation.status == 'O' || reservation.status == 'P')) // Tylko oczekuj¹ce lub potwierdzone
            {
                reservation.status = 'A'; // Zmieniamy status na 'A' (anulowana)
                _context.SaveChanges(); // Zapisujemy zmiany w bazie danych
            }

            var previousReservation = reservation;
            reservation = _context.Reservation.FirstOrDefault(r => r.Id == reservation.Id + 1);

            while (reservation != null && reservation.date == previousReservation.date && reservation.HairdresserId == previousReservation.HairdresserId && previousReservation.ServiceId == reservation.ServiceId && previousReservation.time + TimeSpan.FromMinutes(15) >= reservation.time)
            {
                reservation.status = 'A';
                _context.SaveChanges();
                previousReservation = reservation;
                reservation = _context.Reservation.FirstOrDefault(r => r.Id == reservation.Id + 1);
            }


            // Po anulowaniu, prze³adowujemy stronê, aby zaktualizowaæ widok
            return RedirectToPage();
        }
    }
}
