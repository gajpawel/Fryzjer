using Fryzjer.Data;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Fryzjer.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;

namespace Fryzjer.Pages.AbstractFactory
{
    [IgnoreAntiforgeryToken]
    public class HairdresserScheduleFactoryModel : ScheduleFactoryModel
    {
        private readonly FryzjerContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public List<VacationData> VacationHistory { get; set; }
        public List<Service> HairdresserServices { get; set; }

        public HairdresserScheduleFactoryModel(FryzjerContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public override void OnGet(int week = 0)
        {
            var hairdresserId = _httpContextAccessor.HttpContext?.Session.GetInt32("HairdresserId");
            if (hairdresserId == null)
            {
                Response.Redirect("/Login");
                return;
            }

            CurrentWeek = week;

            // Pobierz tylko us�ugi przypisane do danego fryzjera
            var hairdresserServices = _context.Specialization
                .Where(s => s.HairdresserId == hairdresserId)
                .Include(s => s.Service)
                .Select(s => s.Service)
                .ToList();

            var startDate = DateTime.Now.Date.AddDays(7 * week - (int)DateTime.Now.DayOfWeek + 1);
            WeeklySchedule1 = GenerateSchedule(startDate, hairdresserId.Value);
            WeeklySchedule2 = GenerateSchedule(startDate.AddDays(7), hairdresserId.Value);

            var vacationService = _context.Service.FirstOrDefault(s => s.Name.ToLower() == "urlop");

            var vacationData = _context.Reservation
                .Include(r => r.Hairdresser)
                .ThenInclude(h => h.Place)
                .Where(r => r.ServiceId == vacationService.Id)
                .Where(r => r.HairdresserId == hairdresserId)
                .ToList();

            var groupedVacationData = vacationData
                .GroupBy(r => new { r.date, r.status })
                .ToList();

            VacationHistory = new List<VacationData>();

            foreach (var group in groupedVacationData)
            {
                var startTime = group.Min(r => r.time);
                var endTime = group.Max(r => r.time).Add(new TimeSpan(0, 15, 0));

                VacationHistory.Add(new VacationData
                {
                    date = group.Key.date.ToString("dd-MM-yyyy"),
                    startTime = startTime.ToString(@"hh\:mm"),
                    endTime = endTime.ToString(@"hh\:mm"),
                    status = group.Key.status,
                });
            }
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
            var vacationService = _context.Service.FirstOrDefault(s => s.Name.ToLower() == "urlop");

            var reservations = _context.Reservation
                .Include(r => r.Client)
                .Include(r => r.Service)
                .Where(r => r.date == date && r.HairdresserId == hairdresserId)
                .AsEnumerable()
                .OrderBy(r => r.time.Hours * 60 + r.time.Minutes)
                .ToList();

            TimeBlock currentBlock = null;
            TimeBlock currentVacationBlock = null;

            foreach (var reservation in reservations)
            {
                if (reservation.status == 'A')
                    continue;

                bool isVacation = reservation.ServiceId == vacationService?.Id;

                if (isVacation)
                {
                    if (currentVacationBlock == null ||
                        reservation.time != currentVacationBlock.EndTime ||
                        currentVacationBlock.Status != reservation.status)
                    {
                        if (currentVacationBlock != null)
                        {
                            blocks.Add(currentVacationBlock);
                        }

                        var statusText = reservation.status switch
                        {
                            'O' => "Urlop (oczekuje)",
                            'P' => "Urlop (potwierdzony)",
                            'A' => "Urlop (anulowany)",
                            'Z' => "Urlop (zako�czony)",
                            _ => "Urlop"
                        };

                        currentVacationBlock = new TimeBlock
                        {
                            StartTime = reservation.time,
                            EndTime = reservation.time.Add(new TimeSpan(0, 15, 0)),
                            IsReserved = true,
                            ReservationId = reservation.Id,
                            ClientId = reservation.ClientId,
                            ClientInfo = statusText,
                            ServiceId = reservation.ServiceId,
                            ServiceName = "Urlop",
                            Modal = reservation.status == 'O' ? "#manageReservationModal" : "#deleteReservationModal",
                            Status = reservation.status,
                            BlockClass = $"vacation-{reservation.status.ToString().ToLower()}"
                        };
                    }
                    else
                    {
                        currentVacationBlock.EndTime = reservation.time.Add(new TimeSpan(0, 15, 0));
                    }
                    continue;
                }

                if (currentBlock == null ||
                    reservation.time != currentBlock.EndTime ||
                    currentBlock.ClientId != reservation.ClientId ||
                    currentBlock.ServiceId != reservation.ServiceId)
                {
                    if (currentBlock != null)
                    {
                        blocks.Add(currentBlock);
                    }

                    string modal = "#deleteReservationModal";
                    if (reservation.status == 'O')
                    {
                        modal = "#manageReservationModal";
                    }

                    currentBlock = new TimeBlock
                    {
                        StartTime = reservation.time,
                        EndTime = reservation.time.Add(new TimeSpan(0, 15, 0)),
                        IsReserved = true,
                        ReservationId = reservation.Id,
                        ClientId = reservation.ClientId,
                        ClientInfo = reservation.Client != null ?
                            $"{reservation.Client.Name} {reservation.Client.Surname}\nTel: {reservation.Client.Phone}" :
                            "Zarezerwowane",
                        ServiceId = reservation.ServiceId,
                        ServiceName = reservation.Service?.Name ?? "Brak us�ugi",
                        Modal = modal,
                        Status = reservation.status,
                        BlockClass = "reserved"
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
            if (currentVacationBlock != null)
            {
                blocks.Add(currentVacationBlock);
            }

            return blocks;
        }

        public async Task<IActionResult> OnPostVacationRequestAsync([FromBody] VacationRequest request)
        {
            var hairdresserId = HttpContext.Session.GetInt32("HairdresserId");
            if (hairdresserId == null)
            {
                return RedirectToPage("/Login");
            }

            try
            {
                if (request.date < DateTime.Now.Date)
                {
                    return BadRequest("Nie mo�na utworzy� urlopu z dat� wsteczn�.");
                }

                var vacationService = await _context.Service
                    .FirstOrDefaultAsync(s => s.Name.ToLower() == "urlop");

                if (vacationService == null)
                {
                    return BadRequest("Nie znaleziono us�ugi typu urlop w systemie.");
                }

                TimeSpan startTime, endTime;
                if (request.type == "fullDay")
                {
                    startTime = new TimeSpan(8, 0, 0);
                    endTime = new TimeSpan(18, 0, 0);
                }
                else
                {
                    if (!TimeSpan.TryParse(request.startTime, out startTime) ||
                        !TimeSpan.TryParse(request.endTime, out endTime))
                    {
                        return BadRequest("Nieprawid�owy format czasu.");
                    }

                    if (startTime >= endTime)
                    {
                        return BadRequest("Czas rozpocz�cia musi by� wcze�niejszy ni� czas zako�czenia.");
                    }

                    if (startTime < new TimeSpan(8, 0, 0) || endTime > new TimeSpan(18, 0, 0))
                    {
                        return BadRequest("Godziny urlopu musz� by� mi�dzy 8:00 a 18:00.");
                    }
                }

                var existingReservations = await _context.Reservation
                    .Include(r => r.Client)
                    .Where(r => r.date == request.date &&
                           r.HairdresserId == hairdresserId &&
                           r.status == 'P')
                    .AsNoTracking()
                    .ToListAsync();

                var conflictingReservations = existingReservations
                    .Where(r => (r.time >= startTime && r.time < endTime) ||
                               (r.time <= startTime && r.time.Add(TimeSpan.FromMinutes(15)) > startTime))
                    .ToList();

                var systemClient = await _context.Client
                    .FirstOrDefaultAsync(c => c.Name == "System" && c.Surname == "Urlop");

                if (systemClient == null)
                {
                    systemClient = new Client
                    {
                        Name = "System",
                        Surname = "Urlop",
                        Phone = "000000000",
                        Gender = 'N'
                    };
                    _context.Client.Add(systemClient);
                    await _context.SaveChangesAsync();
                }

                var newReservations = new List<Reservation>();
                var currentTime = startTime;

                while (currentTime < endTime)
                {
                    newReservations.Add(new Reservation
                    {
                        date = request.date,
                        time = currentTime,
                        status = 'O',
                        ClientId = systemClient.Id,
                        HairdresserId = hairdresserId.Value,
                        ServiceId = vacationService.Id
                    });

                    currentTime = currentTime.Add(TimeSpan.FromMinutes(15));
                }

                await _context.Reservation.AddRangeAsync(newReservations);
                await _context.SaveChangesAsync();

                var message = conflictingReservations.Any()
                    ? $"Wniosek o urlop zosta� utworzony. UWAGA: W wybranym terminie istniej� potwierdzone rezerwacje ({conflictingReservations.Count}). " +
                      $"Rezerwacje zostan� anulowane po zatwierdzeniu urlopu."
                    : "Wniosek o urlop zosta� utworzony.";

                return new JsonResult(new
                {
                    success = true,
                    message = message,
                    hasConflicts = conflictingReservations.Any(),
                    conflicts = conflictingReservations.Select(r => new
                    {
                        time = r.time.ToString(@"hh\:mm"),
                        clientName = $"{r.Client?.Name} {r.Client?.Surname}"
                    }).ToList()
                });
            }
            catch (Exception ex)
            {
                return BadRequest($"Wyst�pi� b��d podczas tworzenia urlopu: {ex.Message}");
            }
        }

        public async Task<IActionResult> OnPostCancelVacationRequestAsync([FromBody] VacationRequest request)
        {
            var hairdresserId = HttpContext.Session.GetInt32("HairdresserId");
            if (hairdresserId == null)
            {
                return RedirectToPage("/Login");
            }

            try
            {
                var vacationService = await _context.Service
                    .FirstOrDefaultAsync(s => s.Name.ToLower() == "urlop");

                if (vacationService == null)
                {
                    return BadRequest("Nie znaleziono us�ugi typu urlop w systemie.");
                }

                var vacationRequests = await _context.Reservation
                    .Where(r => r.date == request.date &&
                           r.HairdresserId == hairdresserId &&
                           r.ServiceId == vacationService.Id &&
                           r.status == 'O' &&
                           r.time >= TimeSpan.Parse(request.startTime) &&
                           r.time < TimeSpan.Parse(request.endTime))
                    .ToListAsync();

                if (!vacationRequests.Any())
                {
                    return NotFound("Nie znaleziono oczekuj�cego wniosku urlopowego.");
                }

                foreach (var vacation in vacationRequests)
                {
                    vacation.status = 'A';
                }

                await _context.SaveChangesAsync();
                return new JsonResult(new { success = true });
            }
            catch (Exception ex)
            {
                return BadRequest($"Wyst�pi� b��d podczas anulowania wniosku: {ex.Message}");
            }
        }

        public async Task<IActionResult> OnGetVacationHistoryAsync()
        {
            var hairdresserId = _httpContextAccessor.HttpContext?.Session.GetInt32("HairdresserId");
            if (hairdresserId == null)
            {
                return Unauthorized();
            }

            try
            {
                var vacationService = await _context.Service
                    .FirstOrDefaultAsync(s => s.Name.ToLower() == "urlop");

                if (vacationService == null)
                {
                    return NotFound("Nie znaleziono us�ugi typu urlop");
                }

                var vacationsData = await _context.Reservation
                    .Where(r => r.HairdresserId == hairdresserId &&
                           r.ServiceId == vacationService.Id)
                    .GroupBy(r => new { r.date, r.status })
                    .Select(g => new
                    {
                        date = g.Key.date,
                        status = g.Key.status,
                        startTime = g.Min(r => r.time),
                        endTime = g.Max(r => r.time).Add(TimeSpan.FromMinutes(15))
                    })
                    .ToListAsync();

                var vacations = vacationsData.Select(v => new
                {
                    date = v.date,
                    status = v.status,
                    statusText = GetStatusText(v.status),
                    startTime = v.startTime,
                    endTime = v.endTime
                })
                .OrderByDescending(v => v.date)
                .ThenBy(v => v.startTime)
                .ToList();

                return new JsonResult(vacations);
            }
            catch (Exception ex)
            {
                return BadRequest($"Wyst�pi� b��d podczas pobierania historii urlop�w: {ex.Message}");
            }
        }

        public string GetStatusText(char status)
        {
            return status switch
            {
                'O' => "Oczekuj�cy",
                'P' => "Potwierdzony",
                'A' => "Anulowany",
                'Z' => "Zako�czony",
                _ => "Nieznany"
            };
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

        public async Task<IActionResult> OnPostMarkVacationAsFinishedAsync(int vacationId)
        {
            var hairdresserId = HttpContext.Session.GetInt32("HairdresserId");
            if (hairdresserId == null)
            {
                return RedirectToPage("/Login");
            }

            try
            {
                var vacationReservation = await _context.Reservation
                    .FirstOrDefaultAsync(r => r.Id == vacationId);

                if (vacationReservation == null)
                {
                    return NotFound("Nie znaleziono rezerwacji urlopowej.");
                }

                var allVacationReservations = await _context.Reservation
                    .Where(r => r.date == vacationReservation.date &&
                           r.HairdresserId == hairdresserId &&
                           r.ServiceId == vacationReservation.ServiceId &&
                           r.status == 'P')
                    .ToListAsync();

                foreach (var vacation in allVacationReservations)
                {
                    vacation.status = 'Z';
                }

                await _context.SaveChangesAsync();
                return new JsonResult(new { success = true });
            }
            catch (Exception ex)
            {
                return BadRequest($"Wyst�pi� b��d podczas oznaczania urlopu jako zako�czony: {ex.Message}");
            }
        }

        public class VacationRequest
        {
            public DateTime date { get; set; }
            public string startTime { get; set; }
            public string endTime { get; set; }
            public string type { get; set; }
        }

        public class VacationData
        {
            public string date { get; set; }
            public string startTime { get; set; }
            public string endTime { get; set; }
            public char status { get; set; }
        }
    }
}