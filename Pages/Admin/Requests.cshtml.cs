using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Fryzjer.Data;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Fryzjer.Pages.Admin
{
    public class RequestsModel : PageModel
    {
        private readonly FryzjerContext _context;

        public RequestsModel(FryzjerContext context)
        {
            _context = context;
        }

        public class VacationRequestViewModel
        {
            public int RequestId { get; set; }
            public string HairdresserName { get; set; }
            public string SalonName { get; set; }
            public DateTime Date { get; set; }
            public TimeSpan StartTime { get; set; }
            public TimeSpan EndTime { get; set; }
            public char Status { get; set; }
            public bool HasConflictingReservations { get; set; }
        }

        public List<VacationRequestViewModel> VacationRequests { get; set; }
        public string ErrorMessage { get; set; }
        public string SuccessMessage { get; set; }

        public void OnGet()
        {
            try
            {
                var vacationService = _context.Service.FirstOrDefault(s => s.Name.ToLower() == "urlop");
                if (vacationService == null)
                {
                    ErrorMessage = "Nie znaleziono us³ugi typu urlop w systemie.";
                    return;
                }

                // Pobieramy wszystkie rezerwacje urlopowe z danymi fryzjerów i salonów
                var allVacationRequests = _context.Reservation
                    .Include(r => r.Hairdresser)
                    .ThenInclude(h => h.Place)
                    .Where(r => r.ServiceId == vacationService.Id)
                    .ToList();

                // Grupujemy rezerwacje urlopowe
                var groupedRequests = allVacationRequests
                    .GroupBy(r => new { r.date, r.HairdresserId, r.status })
                    .ToList();

                VacationRequests = new List<VacationRequestViewModel>();

                foreach (var group in groupedRequests)
                {
                    var startTime = group.Min(r => r.time);
                    var endTime = group.Max(r => r.time).Add(new TimeSpan(0, 15, 0));
                    var firstRequest = group.First();

                    // Sprawdzamy koliduj¹ce rezerwacje dla ka¿dego wniosku
                    var normalReservations = _context.Reservation
                        .Where(r => r.date == group.Key.date &&
                                  r.HairdresserId == group.Key.HairdresserId &&
                                  r.ServiceId != vacationService.Id)
                        .ToList();

                    var hasConflicts = normalReservations
                        .Any(r => r.time >= startTime && r.time < endTime);

                    VacationRequests.Add(new VacationRequestViewModel
                    {
                        RequestId = firstRequest.Id,
                        HairdresserName = $"{firstRequest.Hairdresser.Name} {firstRequest.Hairdresser.Surname}",
                        SalonName = firstRequest.Hairdresser.Place?.Name ?? "Brak przypisanego salonu",
                        Date = group.Key.date,
                        StartTime = startTime,
                        EndTime = endTime,
                        Status = group.Key.status,
                        HasConflictingReservations = hasConflicts
                    });
                }

                // Sortowanie wyników
                VacationRequests = VacationRequests
                    .OrderBy(r => r.Date)
                    .ThenBy(r => r.StartTime.Hours * 60 + r.StartTime.Minutes)
                    .ToList();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Wyst¹pi³ b³¹d podczas pobierania wniosków urlopowych: {ex.Message}";
            }
        }

        public IActionResult OnPostConfirm(int requestId)
        {
            try
            {
                var vacationService = _context.Service.FirstOrDefault(s => s.Name.ToLower() == "urlop");
                if (vacationService == null)
                {
                    throw new Exception("Nie znaleziono us³ugi typu urlop w systemie.");
                }

                var mainRequest = _context.Reservation
                    .FirstOrDefault(r => r.Id == requestId);

                if (mainRequest == null)
                {
                    ErrorMessage = "Nie znaleziono wniosku urlopowego.";
                    return RedirectToPage();
                }

                // Pobieramy wszystkie rezerwacje z tego bloku urlopowego
                var vacationBlock = _context.Reservation
                    .Where(r => r.date == mainRequest.date &&
                               r.HairdresserId == mainRequest.HairdresserId &&
                               r.ServiceId == vacationService.Id)
                    .ToList();

                var startTime = vacationBlock.Min(r => r.time);
                var endTime = vacationBlock.Max(r => r.time).Add(new TimeSpan(0, 15, 0));

                // Znajdujemy i usuwamy koliduj¹ce rezerwacje
                var conflictingReservations = _context.Reservation
                    .Where(r => r.date == mainRequest.date &&
                               r.HairdresserId == mainRequest.HairdresserId &&
                               r.ServiceId != vacationService.Id)
                    .ToList()
                    .Where(r => r.time >= startTime && r.time < endTime)
                    .ToList();

                if (conflictingReservations.Any())
                {
                    _context.Reservation.RemoveRange(conflictingReservations);
                }

                // Zatwierdzamy urlop
                foreach (var request in vacationBlock)
                {
                    request.status = 'P'; // Potwierdzony
                }

                _context.SaveChanges();
                SuccessMessage = $"Urlop zosta³ zatwierdzony. Usuniêto {conflictingReservations.Count} koliduj¹cych rezerwacji.";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Wyst¹pi³ b³¹d podczas zatwierdzania urlopu: {ex.Message}";
            }

            return RedirectToPage();
        }

        public IActionResult OnPostReject(int requestId)
        {
            try
            {
                var mainRequest = _context.Reservation
                    .FirstOrDefault(r => r.Id == requestId);

                if (mainRequest == null)
                {
                    ErrorMessage = "Nie znaleziono wniosku urlopowego.";
                    return RedirectToPage();
                }

                // Pobieramy wszystkie rezerwacje z tego bloku urlopowego
                var vacationBlock = _context.Reservation
                    .Where(r => r.date == mainRequest.date &&
                               r.HairdresserId == mainRequest.HairdresserId &&
                               r.ServiceId == mainRequest.ServiceId)
                    .ToList();

                foreach (var request in vacationBlock)
                {
                    request.status = 'A'; // Anulowany
                }

                _context.SaveChanges();
                SuccessMessage = "Urlop zosta³ odrzucony.";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Wyst¹pi³ b³¹d podczas odrzucania urlopu: {ex.Message}";
            }

            return RedirectToPage();
        }
    }
}