using Fryzjer.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Fryzjer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fryzjer.Pages.AbstractFactory
{
    [IgnoreAntiforgeryToken]
    public class HairdresserScheduleFactoryModel : ScheduleFactoryModel
    {
        private readonly FryzjerContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public List<VacationData> VacationHistory { get; set; }

        public HairdresserScheduleFactoryModel(FryzjerContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        // ---------------------------
        // ONGET -> Harmonogram + Historia
        // ---------------------------
        public override void OnGet(int week = 0)
        {
            var hairdresserId = _httpContextAccessor.HttpContext?.Session.GetInt32("HairdresserId");
            if (hairdresserId == null)
            {
                Response.Redirect("/Login");
                return;
            }

            CurrentWeek = week;

            // Pobierz us³ugi fryzjera
            var hairdresserServices = _context.Specialization
                .Where(s => s.HairdresserId == hairdresserId)
                .Include(s => s.Service)
                .Select(s => s.Service)
                .Distinct()
                .ToList();
            Services = hairdresserServices;

            // Generuj harmonogram (2 tygodnie)
            var startDate = DateTime.Now.Date.AddDays(7 * week - (int)DateTime.Now.DayOfWeek + 1);
            WeeklySchedule1 = GenerateSchedule(startDate, hairdresserId.Value);
            WeeklySchedule2 = GenerateSchedule(startDate.AddDays(7), hairdresserId.Value);

            // Historia urlopów
            var vacationService = _context.Service.FirstOrDefault(s => s.Name.ToLower() == "urlop");
            if (vacationService != null)
            {
                var vacationData = _context.Reservation
                    .Where(r => r.HairdresserId == hairdresserId && r.ServiceId == vacationService.Id)
                    .ToList();

                var groupedVacationData = vacationData
                    .GroupBy(r => new { r.date, r.status })
                    .ToList();

                VacationHistory = new List<VacationData>();
                foreach (var group in groupedVacationData)
                {
                    var startT = group.Min(r => r.time);
                    var endT = group.Max(r => r.time).Add(TimeSpan.FromMinutes(15));

                    VacationHistory.Add(new VacationData
                    {
                        date = group.Key.date.ToString("dd-MM-yyyy"),
                        startTime = startT.ToString(@"hh\:mm"),
                        endTime = endT.ToString(@"hh\:mm"),
                        status = group.Key.status
                    });
                }
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

        // ---------------------------
        // GENEROWANIE BLOKÓW - IGNORUJEMY RÓ¯NE STATUSY URLOPU
        // ---------------------------
        private List<TimeBlock> GenerateDailyTimeBlocks(DateTime date, int hairdresserId)
        {
            var blocks = new List<TimeBlock>();
            var vacationService = _context.Service.FirstOrDefault(s => s.Name.ToLower() == "urlop");

            // Sortuj w pamiêci
            var reservations = _context.Reservation
                .Include(r => r.Client)
                .Include(r => r.Service)
                .Where(r => r.date == date && r.HairdresserId == hairdresserId)
                .AsEnumerable()
                .OrderBy(r => r.time) // TimeSpan sort
                .ToList();

            TimeBlock currentBlock = null;
            TimeBlock currentVacationBlock = null;

            foreach (var reservation in reservations)
            {
                if (reservation.status == 'A')
                    continue; // Anulowane pomijamy

                bool isVacation = (vacationService != null && reservation.ServiceId == vacationService.Id);

                if (isVacation)
                {
                    // LOGIKA £¥CZENIA URLOPU W JEDEN BLOK
                    // -> Nie sprawdzamy "if (currentVacationBlock.Status != reservation.status)"
                    //    aby scaliæ nawet O/P/Z w jeden
                    if (currentVacationBlock == null
                        || reservation.time != currentVacationBlock.EndTime)
                    {
                        if (currentVacationBlock != null)
                            blocks.Add(currentVacationBlock);

                        // Nazwa + status
                        string statusText;
                        switch (reservation.status)
                        {
                            case 'O': statusText = "Urlop (oczekuje)"; break;
                            case 'P': statusText = "Urlop (potwierdzony)"; break;
                            case 'Z': statusText = "Urlop (zakoñczony)"; break;
                            default: statusText = "Urlop"; break;
                        }

                        // Modal
                        string modal = (reservation.status == 'O')
                            ? "#manageVacationModal" : "#deleteReservationModal";

                        currentVacationBlock = new TimeBlock
                        {
                            StartTime = reservation.time,
                            EndTime = reservation.time.Add(TimeSpan.FromMinutes(15)),
                            IsReserved = true,
                            ReservationId = reservation.Id,
                            ClientId = (int)reservation.ClientId,
                            ClientInfo = statusText,
                            ServiceId = reservation.ServiceId,
                            ServiceName = "Urlop",
                            Modal = modal,
                            Status = reservation.status,
                            BlockClass = $"vacation-{reservation.status.ToString().ToLower()}"
                        };
                    }
                    else
                    {
                        // Kontynuacja
                        currentVacationBlock.EndTime = currentVacationBlock.EndTime.Add(TimeSpan.FromMinutes(15));
                    }
                }
                else
                {
                    // LOGIKA ZWYK£EJ REZERWACJI
                    bool startNew = false;
                    if (currentBlock == null)
                    {
                        startNew = true;
                    }
                    else
                    {
                        // Sprawdzamy: clientId, serviceId, status, i czas = EndTime
                        if (reservation.time != currentBlock.EndTime
                            || reservation.status != currentBlock.Status
                            || reservation.ClientId != currentBlock.ClientId
                            || reservation.ServiceId != currentBlock.ServiceId)
                        {
                            startNew = true;
                        }
                    }

                    if (startNew)
                    {
                        if (currentBlock != null)
                            blocks.Add(currentBlock);

                        // Modal
                        string modal = (reservation.status == 'O')
                            ? "#manageReservationModal"
                            : "#deleteReservationModal";

                        string clientInfo = (reservation.Client != null)
                            ? $"{reservation.Client.Name} {reservation.Client.Surname}\nTel: {reservation.Client.Phone}"
                            : "Zarezerwowane";

                        currentBlock = new TimeBlock
                        {
                            StartTime = reservation.time,
                            EndTime = reservation.time.Add(TimeSpan.FromMinutes(15)),
                            IsReserved = true,
                            ReservationId = reservation.Id,
                            ClientId = (int)reservation.ClientId,
                            ClientInfo = clientInfo,
                            ServiceId = reservation.ServiceId,
                            ServiceName = reservation.Service?.Name ?? "Brak us³ugi",
                            Modal = modal,
                            Status = reservation.status,
                            BlockClass = "reserved"
                        };
                    }
                    else
                    {
                        currentBlock.EndTime = currentBlock.EndTime.Add(TimeSpan.FromMinutes(15));
                    }
                }
            }

            // Dodaj ostatnie
            if (currentBlock != null)
                blocks.Add(currentBlock);
            if (currentVacationBlock != null)
                blocks.Add(currentVacationBlock);

            return blocks;
        }

        // ---------------------------
        // WNIOSEK O URLOP - LOGIKA UNIKANIA CZÊŒCIOWEGO OVERLAPU
        // ---------------------------
        [HttpPost]
        public async Task<IActionResult> OnPostVacationRequestAsync([FromBody] VacationRequest request)
        {
            var hairdresserId = HttpContext.Session.GetInt32("HairdresserId");
            if (hairdresserId == null)
            {
                return RedirectToPage("/Login");
            }

            try
            {
                var startDate = request.startDate.Date;
                var endDate = request.endDate.Date;
                if (startDate > endDate)
                {
                    return BadRequest("Data pocz¹tkowa jest póŸniejsza ni¿ koñcowa.");
                }
                if (startDate < DateTime.Now.Date)
                {
                    return BadRequest("Nie mo¿na utworzyæ urlopu w przesz³oœci.");
                }

                var vacationService = await _context.Service
                    .FirstOrDefaultAsync(s => s.Name.ToLower() == "urlop");
                if (vacationService == null)
                {
                    return BadRequest("Nie znaleziono us³ugi 'Urlop'.");
                }

                // Ustal godziny
                TimeSpan st, et;
                if (request.type == "fullDay")
                {
                    st = new TimeSpan(8, 0, 0);
                    et = new TimeSpan(18, 0, 0);
                }
                else
                {
                    if (!TimeSpan.TryParse(request.startTime, out st) ||
                        !TimeSpan.TryParse(request.endTime, out et))
                    {
                        return BadRequest("Nieprawid³owy format godziny.");
                    }
                    if (st >= et)
                    {
                        return BadRequest("Godzina rozpoczêcia musi byæ wczeœniejsza ni¿ zakoñczenia.");
                    }
                }

                // Klient "System Urlop"
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

                // Dla ka¿dego dnia w [startDate..endDate] sprawdŸ istn. urlop
                for (var day = startDate; day <= endDate; day = day.AddDays(1))
                {
                    var existing = await _context.Reservation
                        .Where(r => r.HairdresserId == hairdresserId
                                 && r.ServiceId == vacationService.Id
                                 && (r.status == 'O' || r.status == 'P') // realnie blokuje
                                 && r.date == day)
                        .ToListAsync();

                    if (!existing.Any())
                    {
                        // Nic nie ma -> tworzymy
                        var newSlots = CreateVacationSlots(day, st, et, systemClient.Id, hairdresserId.Value, vacationService.Id);
                        _context.Reservation.AddRange(newSlots);
                    }
                    else
                    {
                        // ZnajdŸ minimalny i maksymalny zakres ju¿ istniej¹cego urlopu
                        var oldMin = existing.Min(r => r.time);
                        var oldMax = existing.Max(r => r.time).Add(TimeSpan.FromMinutes(15));

                        var oldRange = (start: oldMin, end: oldMax);
                        var newRange = (start: st, end: et);

                        bool newContainsOld = (newRange.start <= oldRange.start && newRange.end >= oldRange.end);
                        bool oldContainsNew = (oldRange.start <= newRange.start && oldRange.end >= newRange.end);
                        bool overlap = !(newRange.end <= oldRange.start || newRange.start >= oldRange.end);

                        if (newContainsOld)
                        {
                            // Nowy zawiera stary
                            bool anyConfirmed = existing.Any(r => r.status == 'P');
                            if (anyConfirmed)
                            {
                                return BadRequest($"Dnia {day:dd.MM.yyyy} istnieje ju¿ potwierdzony urlop, nie mo¿na go nadpisaæ.");
                            }
                            // Usuwamy stare (status 'O') i wstawiamy wiêkszy
                            _context.Reservation.RemoveRange(existing);

                            var newSlots = CreateVacationSlots(day, newRange.start, newRange.end,
                                systemClient.Id, hairdresserId.Value, vacationService.Id);
                            _context.Reservation.AddRange(newSlots);
                        }
                        else if (oldContainsNew)
                        {
                            // Stary wiêkszy -> b³¹d
                            return BadRequest($"Dnia {day:dd.MM.yyyy} istnieje ju¿ urlop, który obejmuje wybrany zakres.");
                        }
                        else
                        {
                            // Czêœciowe nak³adanie -> b³¹d
                            if (overlap)
                            {
                                return BadRequest($"Dnia {day:dd.MM.yyyy} urlop czêœciowo nachodzi na istniej¹cy.");
                            }
                            else
                            {
                                // Brak overlap -> mo¿na dodaæ dodatkowy blok
                                var newSlots = CreateVacationSlots(day, newRange.start, newRange.end,
                                    systemClient.Id, hairdresserId.Value, vacationService.Id);
                                _context.Reservation.AddRange(newSlots);
                            }
                        }
                    }
                }

                await _context.SaveChangesAsync();
                return new JsonResult(new
                {
                    success = true,
                    message = $"Urlop utworzony/zmieniony w zakresie {startDate:dd.MM.yyyy} - {endDate:dd.MM.yyyy}."
                });
            }
            catch (Exception ex)
            {
                return BadRequest($"B³¹d: {ex.Message}");
            }
        }

        private List<Reservation> CreateVacationSlots(
            DateTime day, TimeSpan startTime, TimeSpan endTime,
            int clientId, int hairdresserId, int serviceId)
        {
            var slots = new List<Reservation>();
            var current = startTime;
            while (current < endTime)
            {
                slots.Add(new Reservation
                {
                    date = day,
                    time = current,
                    status = 'O', // Oczekuj¹cy
                    ClientId = clientId,
                    HairdresserId = hairdresserId,
                    ServiceId = serviceId
                });
                current = current.Add(TimeSpan.FromMinutes(15));
            }
            return slots;
        }

        // ---------------------------
        // ANULOWANIE WNIOSKU (WYCOFANIE)
        // ---------------------------
        [HttpPost]
        public async Task<IActionResult> OnPostCancelVacationRequestAsync([FromBody] CancelVacationRequest request)
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
                    return BadRequest("Brak us³ugi 'urlop'.");
                }

                var startDate = request.startDate.Date;
                var endDate = request.endDate.Date;

                TimeSpan st, et;
                if (request.type == "fullDay")
                {
                    st = new TimeSpan(8, 0, 0);
                    et = new TimeSpan(18, 0, 0);
                }
                else
                {
                    if (!TimeSpan.TryParse(request.startTime, out st) ||
                        !TimeSpan.TryParse(request.endTime, out et))
                    {
                        return BadRequest("B³êdny format godzin.");
                    }
                }

                for (var day = startDate; day <= endDate; day = day.AddDays(1))
                {
                    var toCancel = await _context.Reservation
                        .Where(r => r.HairdresserId == hairdresserId
                                 && r.ServiceId == vacationService.Id
                                 && r.status == 'O'
                                 && r.date == day
                                 && r.time >= st
                                 && r.time < et)
                        .ToListAsync();

                    foreach (var slot in toCancel)
                    {
                        slot.status = 'A'; // Anulowany
                    }
                }

                await _context.SaveChangesAsync();
                return new JsonResult(new
                {
                    success = true,
                    message = "Wniosek urlopowy wycofany."
                });
            }
            catch (Exception ex)
            {
                return BadRequest($"B³¹d anulowania: {ex.Message}");
            }
        }

        // ---------------------------
        // HISTORIA URLOPÓW
        // ---------------------------
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
                    return NotFound("Brak us³ugi 'urlop'.");
                }

                var vacData = await _context.Reservation
                    .Where(r => r.HairdresserId == hairdresserId && r.ServiceId == vacationService.Id)
                    .GroupBy(r => new { r.date, r.status })
                    .Select(g => new
                    {
                        date = g.Key.date,
                        status = g.Key.status,
                        startTime = g.Min(r => r.time),
                        endTime = g.Max(r => r.time).Add(TimeSpan.FromMinutes(15))
                    })
                    .ToListAsync();

                var result = vacData
                    .Select(v => new
                    {
                        date = v.date,
                        status = v.status,
                        statusText = GetStatusText(v.status),
                        startTime = v.startTime,
                        endTime = v.endTime
                    })
                    .OrderByDescending(x => x.date)
                    .ThenBy(x => x.startTime)
                    .ToList();

                return new JsonResult(result);
            }
            catch (Exception ex)
            {
                return BadRequest($"B³¹d pobierania historii: {ex.Message}");
            }
        }

        // ---------------------------
        // ZWYCZAJNE REZERWACJE (ANULOWANIE/POTWIERDZENIE)
        // ---------------------------
        public IActionResult OnPostDeleteReservation(int reservationId)
        {
            var reservation = _context.Reservation.FirstOrDefault(r => r.Id == reservationId);
            if (reservation != null && (reservation.status == 'O' || reservation.status == 'P'))
            {
                reservation.status = 'A';
                _context.SaveChanges();

                // Usuñ kolejne sloty, jeœli to d³u¿sza rezerwacja
                var tempId = reservationId + 1;
                var next = _context.Reservation.FirstOrDefault(r => r.Id == tempId);
                while (next != null &&
                       next.ClientId == reservation.ClientId &&
                       next.date == reservation.date &&
                       next.ServiceId == reservation.ServiceId &&
                       (next.status == 'O' || next.status == 'P') &&
                       reservation.time + TimeSpan.FromMinutes(15) >= next.time)
                {
                    next.status = 'A';
                    _context.SaveChanges();
                    tempId++;
                    reservation = next;
                    next = _context.Reservation.FirstOrDefault(r => r.Id == tempId);
                }
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

                // PotwierdŸ kolejne sloty
                var tempId = reservationId + 1;
                var next = _context.Reservation.FirstOrDefault(r => r.Id == tempId);
                while (next != null &&
                       next.ClientId == reservation.ClientId &&
                       next.date == reservation.date &&
                       next.ServiceId == reservation.ServiceId &&
                       next.status == 'O' &&
                       reservation.time + TimeSpan.FromMinutes(15) >= next.time)
                {
                    next.status = 'P';
                    _context.SaveChanges();
                    tempId++;
                    reservation = next;
                    next = _context.Reservation.FirstOrDefault(r => r.Id == tempId);
                }
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
                var vac = await _context.Reservation.FirstOrDefaultAsync(r => r.Id == vacationId);
                if (vac == null)
                {
                    return NotFound("Nie znaleziono rezerwacji urlopowej.");
                }

                var allVacationReservations = await _context.Reservation
                    .Where(r => r.HairdresserId == hairdresserId
                             && r.ServiceId == vac.ServiceId
                             && r.date == vac.date
                             && r.status == 'P')
                    .ToListAsync();

                foreach (var v in allVacationReservations)
                {
                    v.status = 'Z'; // Zakoñczony
                }
                await _context.SaveChangesAsync();
                return new JsonResult(new { success = true });
            }
            catch (Exception ex)
            {
                return BadRequest($"B³¹d oznaczania urlopu jako zakoñczonego: {ex.Message}");
            }
        }

        // ---------------------------
        // Pomocnicze klasy
        // ---------------------------
        public class VacationRequest
        {
            public DateTime startDate { get; set; }
            public DateTime endDate { get; set; }
            public string startTime { get; set; }
            public string endTime { get; set; }
            public string type { get; set; } // "fullDay"/"customPeriod"
        }

        public class CancelVacationRequest
        {
            public DateTime startDate { get; set; }
            public DateTime endDate { get; set; }
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

        public string GetStatusText(char status)
        {
            return status switch
            {
                'O' => "Oczekuj¹cy",
                'P' => "Potwierdzony",
                'A' => "Anulowany",
                'Z' => "Zakoñczony",
                _ => "Nieznany"
            };
        }
    }
}
