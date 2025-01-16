using Fryzjer.Data;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Fryzjer.Models;

namespace Fryzjer.Pages.AbstractFactory
{
    /// <summary>
    /// Klasa implementuj¹ca fabrykê harmonogramu dla fryzjera
    /// </summary>
    public class HairdresserScheduleFactoryModel : ScheduleFactoryModel
    {
        private readonly FryzjerContext _context;

        public HairdresserScheduleFactoryModel(FryzjerContext context)
        {
            _context = context;
        }

        // Implementacja metody abstrakcyjnej z klasy bazowej
        public override void OnGet(int week = 0)
        {
            // Pobieranie ID zalogowanego fryzjera
            int? hairdresserId = HttpContext.Session.GetInt32("HairdresserId");
            if (hairdresserId == null)
            {
                Response.Redirect("/Login");
                return;
            }

            CurrentWeek = week;

            // Pobierz wszystkie us³ugi dla rozwijanej listy w modalu
            Services = _context.Service.ToList();

            // Oblicz pierwszy dzieñ tygodnia (poniedzia³ek)
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

            var vacationService = _context.Service.FirstOrDefault(s => s.Name.ToLower() == "urlop");

            var reservations = _context.Reservation
                .Include(r => r.Client)
                .Include(r => r.Service)
                .Where(r => r.date == date && r.HairdresserId == hairdresserId && r.status != 'A')
                .AsEnumerable()
                .OrderBy(r => r.time.Hours * 60 + r.time.Minutes)
                .ToList();

            TimeBlock currentBlock = null;
            foreach (var reservation in reservations)
            {
                bool isVacation = reservation.ServiceId == vacationService?.Id;
                bool shouldCreateNewBlock = currentBlock == null ||
                                          reservation.time != currentBlock.EndTime ||
                                          (!isVacation && currentBlock.ClientId != reservation.ClientId) ||
                                          currentBlock.ServiceId != reservation.ServiceId;

                if (shouldCreateNewBlock)
                {
                    if (currentBlock != null)
                    {
                        blocks.Add(currentBlock);
                    }

                    string modal = (reservation.status == 'O' && !isVacation) ?
                        "#manageReservationModal" : "#deleteReservationModal";

                    string clientInfo;
                    if (isVacation)
                    {
                        var statusText = reservation.status switch
                        {
                            'O' => "Urlop (oczekuje)",
                            'P' => "Urlop (potwierdzony)",
                            'A' => "Urlop (anulowany)",
                            _ => "Urlop"
                        };
                        clientInfo = statusText;
                    }
                    else
                    {
                        clientInfo = reservation.Client != null ?
                            $"{reservation.Client.Name} {reservation.Client.Surname}\nTel: {reservation.Client.Phone}" :
                            "Zarezerwowane";
                    }

                    currentBlock = new TimeBlock
                    {
                        StartTime = reservation.time,
                        EndTime = reservation.time.Add(new TimeSpan(0, 15, 0)),
                        IsReserved = true,
                        ReservationId = reservation.Id,
                        ClientId = reservation.ClientId,
                        ClientInfo = clientInfo,
                        ServiceId = reservation.ServiceId,
                        ServiceName = reservation.Service?.Name ?? "Brak us³ugi",
                        Modal = modal,
                        Status = reservation.status,
                        BlockClass = isVacation ? $"vacation-{reservation.status.ToString().ToLower()}" : "reserved"
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

        public IActionResult OnPostDeleteReservation(int reservationId)
        {
            var reservation = _context.Reservation.FirstOrDefault(r => r.Id == reservationId);

            if (reservation != null && (reservation.status == 'O' || reservation.status == 'P'))
            {
                reservation.status = 'A';
                _context.SaveChanges();

                var tempId = reservationId + 1;
                var nextReservation = _context.Reservation.FirstOrDefault(r => r.Id == tempId);

                while (nextReservation != null &&
                       reservation != null &&
                       nextReservation.ClientId == reservation.ClientId &&
                       nextReservation.date == reservation.date &&
                       reservation.ServiceId == nextReservation.ServiceId &&
                       reservation.time + TimeSpan.FromMinutes(15) >= nextReservation.time)
                {
                    nextReservation.status = 'A';
                    tempId += 1;
                    reservation = nextReservation;
                    nextReservation = _context.Reservation.FirstOrDefault(r => r.Id == tempId);
                }
                _context.SaveChanges();
            }

            return RedirectToPage();
        }

        public IActionResult OnPostConfirmReservation(int reservationId)
        {
            var reservation = _context.Reservation.FirstOrDefault(r => r.Id == reservationId);

            if (reservation != null && reservation.status == 'O')
            {
                reservation.status = 'P';
                _context.SaveChanges();

                var tempId = reservationId + 1;
                var nextReservation = _context.Reservation.FirstOrDefault(r => r.Id == tempId);

                while (nextReservation != null &&
                       reservation != null &&
                       nextReservation.ClientId == reservation.ClientId &&
                       nextReservation.date == reservation.date &&
                       reservation.ServiceId == nextReservation.ServiceId &&
                       reservation.time + TimeSpan.FromMinutes(15) >= nextReservation.time)
                {
                    nextReservation.status = 'P';
                    tempId += 1;
                    reservation = nextReservation;
                    nextReservation = _context.Reservation.FirstOrDefault(r => r.Id == tempId);
                }
                _context.SaveChanges();
            }

            return RedirectToPage();
        }

        public IActionResult OnPostVacationRequest(DateTime date, TimeSpan startTime, TimeSpan endTime)
        {
            int? hairdresserId = HttpContext.Session.GetInt32("HairdresserId");
            if (hairdresserId == null)
            {
                return RedirectToPage("/Login");
            }

            try
            {
                var vacationService = _context.Service.FirstOrDefault(s => s.Name.ToLower() == "urlop");
                if (vacationService == null)
                {
                    throw new Exception("Nie znaleziono us³ugi typu urlop w systemie.");
                }

                var existingReservations = _context.Reservation
                    .Any(r => r.date == date &&
                             r.HairdresserId == hairdresserId &&
                             ((r.time >= startTime && r.time < endTime) ||
                              (r.time.Add(new TimeSpan(0, 15, 0)) > startTime && r.time < endTime)));

                if (existingReservations)
                {
                    throw new Exception("Istniej¹ ju¿ rezerwacje w wybranym czasie.");
                }

                var currentTime = startTime;
                while (currentTime < endTime)
                {
                    var reservation = new Reservation
                    {
                        date = date,
                        time = currentTime,
                        status = 'O',
                        ClientId = 0,
                        HairdresserId = hairdresserId.Value,
                        ServiceId = vacationService.Id
                    };

                    _context.Reservation.Add(reservation);
                    currentTime = currentTime.Add(new TimeSpan(0, 15, 0));
                }

                _context.SaveChanges();
                return RedirectToPage();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToPage();
            }
        }
    }
}