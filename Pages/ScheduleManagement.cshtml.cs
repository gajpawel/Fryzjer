using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Fryzjer.Data;
using Fryzjer.Pages.AbstractFactory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fryzjer.Models;

namespace Fryzjer.Pages
{
    public class ScheduleManagementModel : PageModel
    {
        private readonly FryzjerContext _context;
        private readonly ILogger<ScheduleManagementModel> _logger;
        private readonly ScheduleFactory _scheduleFactory;

        public ScheduleManagementModel(FryzjerContext context, ILogger<ScheduleManagementModel> logger)
        {
            _context = context;
            _logger = logger;
            _scheduleFactory = new ClientScheduleFactory(this, context);
        }

        [BindProperty(SupportsGet = true)]
        public int WeekOffset { get; set; } = 0;

        [BindProperty]
        public Hairdresser? SelectedHairdresser { get; set; }

        [BindProperty]
        public int? SelectedHairdresserId { get; set; }

        public SelectList HairdressersList { get; set; }
        public List<DailySchedule> WeeklySchedule { get; set; } = new();

        public DateTime WeekStartDate
        {
            get
            {
                var date = DateTime.Now.Date;
                while (date.DayOfWeek != DayOfWeek.Monday)
                {
                    date = date.AddDays(-1);
                }
                return date.AddDays(WeekOffset * 7);
            }
        }

        public async Task<IActionResult> OnGetAsync(int? hairdresserId = null, int weekOffset = 0)
        {
            WeekOffset = weekOffset;

            var hairdressers = await _context.Hairdresser
                .OrderBy(h => h.Name)
                .ThenBy(h => h.Surname)
                .ToListAsync();

            HairdressersList = new SelectList(
                hairdressers.Select(h => new
                {
                    h.Id,
                    FullName = $"{h.Name} {h.Surname}"
                }),
                "Id",
                "FullName"
            );

            SelectedHairdresserId = hairdresserId ?? hairdressers.FirstOrDefault()?.Id;

            if (SelectedHairdresserId.HasValue)
            {
                SelectedHairdresser = await _context.Hairdresser
                    .Include(h => h.Place)
                    .FirstOrDefaultAsync(h => h.Id == SelectedHairdresserId);

                var operations = _scheduleFactory.CreateSchedule();
                var (weeklySchedule1, weeklySchedule2) = operations.CreateSchedule(SelectedHairdresserId.Value, WeekStartDate);
                WeeklySchedule = weeklySchedule1.Concat(weeklySchedule2).ToList();
            }

            return Page();
        }
    }
}
