using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Fryzjer.Data;
using Fryzjer.Pages.AbstractFactory; // Importowanie przestrzeni nazw
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
        private readonly AdminScheduleFactory _scheduleFactory;

        public ScheduleManagementModel(FryzjerContext context, ILogger<ScheduleManagementModel> logger)
        {
            _context = context;
            _logger = logger;
            _scheduleFactory = new AdminScheduleFactory(this, context); // Użycie nowej fabryki
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

                // Filtruj tylko dni bieżącego tygodnia
                WeeklySchedule = MergeConsecutiveBlocks(
                    weeklySchedule1
                        .Concat(weeklySchedule2)
                        .Where(day => day.Date >= WeekStartDate && day.Date < WeekStartDate.AddDays(7))
                        .ToList()
                );
            }

            return Page();
        }


        private List<DailySchedule> MergeConsecutiveBlocks(List<DailySchedule> schedule)
        {
            foreach (var day in schedule)
            {
                var mergedBlocks = new List<TimeBlock>();
                TimeBlock currentBlock = null;

                foreach (var block in day.TimeBlocks.OrderBy(b => b.StartTime))
                {
                    if (currentBlock == null)
                    {
                        currentBlock = block;
                    }
                    else
                    {
                        bool canMerge = currentBlock.EndTime == block.StartTime &&
                                       currentBlock.ServiceId == block.ServiceId &&
                                       currentBlock.ClientId == block.ClientId &&
                                       currentBlock.Status == block.Status;

                        if (canMerge)
                        {
                            currentBlock.EndTime = block.EndTime;
                        }
                        else
                        {
                            mergedBlocks.Add(currentBlock);
                            currentBlock = block;
                        }
                    }
                }

                if (currentBlock != null)
                {
                    mergedBlocks.Add(currentBlock);
                }

                day.TimeBlocks = mergedBlocks;
            }

            return schedule;
        }
    }
}
