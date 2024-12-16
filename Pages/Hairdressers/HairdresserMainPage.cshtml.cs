using Microsoft.AspNetCore.Mvc.RazorPages;
using Fryzjer.Models;
using Fryzjer.Data;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;

namespace Fryzjer.Pages.Hairdressers
{
    public class HairdresserMainPageModel : PageModel
    {
        private readonly FryzjerContext _context;

        public HairdresserMainPageModel(FryzjerContext context)
        {
            _context = context;
        }

        // Harmonogram na bie��cy tydzie�
        public List<DailySchedule> WeeklySchedule1 { get; set; } = new List<DailySchedule>();
        // Harmonogram na kolejny tydzie�
        public List<DailySchedule> WeeklySchedule2 { get; set; } = new List<DailySchedule>();
        // Numer wy�wietlanego tygodnia (0 = bie��cy, -1 = poprzedni, 1 = nast�pny)
        public int CurrentWeek { get; set; } = 0;
        // Lista us�ug do wy�wietlenia w widoku
        public List<Service> Services { get; set; } = new List<Service>();

        // Metoda pomocnicza do formatowania czasu
        public static string FormatTime(TimeSpan time)
        {
            int hours = time.Hours;
            int minutes = time.Minutes;
            return $"{hours:00}:{minutes:00}";
        }

        [HttpPost]
        [Route("api/reservation/{id}/confirm")]
        public IActionResult ConfirmReservation(int id)
        {
            var reservation = _context.Reservation.FirstOrDefault(r => r.Id == id);
            if (reservation == null)
            {
                return NotFound("Rezerwacja nie zosta�a znaleziona.");
            }
            if (reservation.status == 'P')
            {
                return BadRequest("Rezerwacja jest ju� potwierdzona.");
            }
            reservation.status = 'P';
            _context.SaveChanges();
            return new JsonResult(new { success = true, message = "Rezerwacja zosta�a potwierdzona." });
        }

        public void OnGet(int week = 0)
        {
            int? hairdresserId = HttpContext.Session.GetInt32("HairdresserId");
            if (hairdresserId == null)
            {
                Response.Redirect("/Login");
                return;
            }

            CurrentWeek = week;
            Services = _context.Service.ToList();

            var startDate = DateTime.Now.Date.AddDays(7 * week - (int)DateTime.Now.DayOfWeek + 1);
            WeeklySchedule1 = GenerateSchedule(startDate, hairdresserId.Value);
            WeeklySchedule2 = GenerateSchedule(startDate.AddDays(7), hairdresserId.Value);
        }

        private List<DailySchedule> GenerateSchedule(DateTime startDate, int hairdresserId)
        {
            var schedule = new List<DailySchedule>();
            for (int i = 0; i < 5; i++)
            {
                var date = startDate.AddDays(i);
                var timeBlocks = new List<TimeBlock>();
                if (date >= DateTime.Now.Date)
                {
                    timeBlocks = GenerateDailyTimeBlocks(date, hairdresserId);
                }
                schedule.Add(new DailySchedule
                {
                    Date = date,
                    TimeBlocks = timeBlocks
                });
            }
            return schedule;
        }

        private List<TimeBlock> GenerateDailyTimeBlocks(DateTime date, int hairdresserId)
        {
            var blocks = new List<TimeBlock>();
            var startTime = new TimeSpan(8, 0, 0);
            var endTime = new TimeSpan(18, 0, 0);

            var reservations = _context.Reservation
                .Include(r => r.Client)
                .Include(r => r.Service)
                .Where(r => r.date == date && r.HairdresserId == hairdresserId)
                .ToList()
                .OrderBy(r => r.time)
                .ToList();

            TimeBlock currentBlock = null;
            foreach (var reservation in reservations)
            {
                if (currentBlock == null || reservation.time != currentBlock.EndTime)
                {
                    if (currentBlock != null)
                    {
                        blocks.Add(currentBlock);
                    }
                    string modal;
                    if (reservation.status == 'O' && reservation.ServiceId!=4)
                    {
                        modal = "#manageReservationModal";
                    }
                    else modal = "#deleteReservationModal";
                    currentBlock = new TimeBlock
                    {
                        StartTime = reservation.time,
                        EndTime = reservation.time.Add(new TimeSpan(0, 15, 0)),
                        IsReserved = true,
                        ReservationId = reservation.Id,
                        ClientInfo = $"{reservation.Client?.Name} {reservation.Client?.Surname}\nTel: {reservation.Client?.Phone}",
                        ServiceName = reservation.Service?.Name ?? "Brak us�ugi",
                        Modal = modal,
                        Status= reservation.status
                    };
                }
                else
                {
                    currentBlock.EndTime = currentBlock.EndTime.Add(new TimeSpan(0, 15, 0));
                }
            }
            if (currentBlock != null)
            {
                blocks.Add(currentBlock);
            }
            return blocks;
        }
    }

    public class DailySchedule
    {
        public DateTime Date { get; set; }
        public List<TimeBlock> TimeBlocks { get; set; } = new List<TimeBlock>();
    }

    public class TimeBlock
    {
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public bool IsReserved { get; set; }
        public string TimeRange
        {
            get
            {
                var startTimeStr = $"{StartTime.Hours:00}:{StartTime.Minutes:00}";
                var endTimeStr = $"{EndTime.Hours:00}:{EndTime.Minutes:00}";
                return $"{startTimeStr} - {endTimeStr}";
            }
        }
        public string? ClientInfo { get; set; }
        public int? ReservationId { get; set; }
        public string? ServiceName { get; set; }
        public string? Modal { get; set; }
        public char? Status { get; set; }
    }
}