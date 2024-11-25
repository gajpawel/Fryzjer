using Fryzjer.Models;
using Fryzjer.OtherClasses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System;

namespace Fryzjer.Pages
{
    public class IndexModel : PageModel
    {
        private readonly Fryzjer.Data.FryzjerContext _context;
        private readonly FileChecker _fileChecker;

        public IList<Place> Place { get; set; } = default!;

        public IndexModel(Fryzjer.Data.FryzjerContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _fileChecker = new FileChecker(environment);
        }

        public async Task OnGetAsync()
        {
            Place = await _context.Place.ToListAsync();
        }

        public IActionResult OnPostLogout()
        {
            // Usuñ sesjê u¿ytkownika
            HttpContext.Session.Remove("UserLogin");
            HttpContext.Session.Remove("HairdresserId");
            HttpContext.Session.Remove("UserType");
            return RedirectToPage("/Index"); // Przekierowanie na stronê g³ówn¹
        }

        public bool IsFileAvailable(string? logoPath)
        {
            return _fileChecker.DoesFileExist(logoPath);
        }

    }
}