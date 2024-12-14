using Fryzjer.Data;
using Fryzjer.Models;
using Fryzjer.OtherClasses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System;

namespace Fryzjer.Pages
{
	public class ViewSalonModel : PageModel
	{
		private readonly FryzjerContext _context;
		private readonly FileChecker _fileChecker;

		public ViewSalonModel(Fryzjer.Data.FryzjerContext context, IWebHostEnvironment environment)
		{
			_context = context;
			_fileChecker = new FileChecker(environment);
		}

		[BindProperty]
		public int SalonId { get; set; }

		public Place? Place { get; set; }
		public List<Hairdresser> Hairdressers { get; set; } = [];

		public List<Service?> Services { get; set; } = [];

		public async Task<IActionResult> OnGetAsync(int id)
		{
			SalonId = id;

			Place = await _context.Place
				.FirstOrDefaultAsync(h => h.Id == id);

			if (Place == null)
			{
				return NotFound();
			}

			Hairdressers = await _context.Hairdresser
				.Where(h => h.PlaceId == id)
				.ToListAsync();

			Services = await _context.Specialization
				.Include(s => s.Service)
				.Include(s => s.Hairdresser)
				.Where(s => s.Hairdresser.PlaceId == id)
				.Select(s => s.Service)
				.Distinct()
				.ToListAsync();

			return Page();
		}

		public bool IsFileAvailable(string? logoPath)
		{
			return _fileChecker.DoesFileExist(logoPath);
		}

		public bool CheckClientSession()
		{
			var userType = HttpContext.Session.GetString("UserType");
            if (userType != null && userType.Equals("Client"))
				return true;
            else
				return false;
        }
	}
}
