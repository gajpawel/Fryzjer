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
    public class HairdresserScheduleFactoryModel : PageModel, IScheduleOperations
    {
        private readonly FryzjerContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ScheduleFactory _scheduleFactory;

        public List<VacationData> VacationHistory { get; set; }
        public List<DailySchedule> WeeklySchedule1 { get; set; } = new List<DailySchedule>();
        public List<DailySchedule> WeeklySchedule2 { get; set; } = new List<DailySchedule>();
        public int CurrentWeek { get; set; }
        public List<Service> Services { get; set; } = new List<Service>();

        public HairdresserScheduleFactoryModel(FryzjerContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _scheduleFactory = new ConcreteScheduleFactory(this);
        }

        public class ConcreteScheduleFactory : ScheduleFactory
        {
            public ConcreteScheduleFactory(PageModel pageModel) : base(pageModel) { }

            public override IScheduleOperations CreateSchedule()
            {
                return _pageModel as IScheduleOperations;
            }
        }
        public void OnGet(int week = 0)
        {
            var hairdresserId = _httpContextAccessor.HttpContext?.Session.GetInt32("HairdresserId");
            if (hairdresserId == null)
            {
                Response.Redirect("/Login");
                return;
            }

            CurrentWeek = week;

            var hairdresserServices = _context.Specialization
                .Where(s => s.HairdresserId == hairdresserId)
                .Include(s => s.Service)
                .Select(s => s.Service)
                .Distinct()
                .ToList();
            Services = hairdresserServices;

            var startDate = DateTime.Now.Date.AddDays(7 * week);

            if (startDate.DayOfWeek == DayOfWeek.Saturday || startDate.DayOfWeek == DayOfWeek.Sunday)
            {
                startDate = startDate.AddDays(((int)DayOfWeek.Monday - (int)startDate.DayOfWeek + 7) % 7);
            }
            else
            {
                startDate = startDate.AddDays(-(int)startDate.DayOfWeek + (int)DayOfWeek.Monday);
            }

            var scheduleOperations = _scheduleFactory.CreateSchedule();
            var (schedule1, schedule2) = scheduleOperations.CreateSchedule(hairdresserId.Value, startDate);

            WeeklySchedule1 = schedule1;
            WeeklySchedule2 = schedule2;

            LoadVacationHistory(hairdresserId.Value);
        }

        public (List<DailySchedule>, List<DailySchedule>) CreateSchedule(int hairdresserId, DateTime startDate)
        {
            var schedule1 = new List<DailySchedule>();
            var schedule2 = new List<DailySchedule>();

            for (int i = 0; i < 5; i++)
            {
                var date = startDate.AddDays(i);
                schedule1.Add(new DailySchedule
                {
                    Date = date,
                    TimeBlocks = GenerateDailyTimeBlocks(date, hairdresserId)
                });
            }

            var nextWeek = startDate.AddDays(7);
            for (int i = 0; i < 5; i++)
            {
                var date = nextWeek.AddDays(i);
                schedule2.Add(new DailySchedule
                {
                    Date = date,
                    TimeBlocks = GenerateDailyTimeBlocks(date, hairdresserId)
                });
            }

            return (schedule1, schedule2);
        }
        private List<TimeBlock> GenerateDailyTimeBlocks(DateTime date, int hairdresserId)
        {
            var blocks = new List<TimeBlock>();

            if (date.Date < DateTime.Now.Date.AddDays(-((int)DateTime.Now.DayOfWeek - 1)))
            {
                return blocks;
            }

            var vacationService = _context.Service.FirstOrDefault(s => s.Name.ToLower() == "urlop");

            var reservations = _context.Reservation
                .Include(r => r.Client)
                .Include(r => r.Service)
                .Where(r => r.date == date && r.HairdresserId == hairdresserId)
                .AsEnumerable()
                .OrderBy(r => r.time)
                .ToList();

            TimeBlock currentBlock = null;
            TimeBlock currentVacationBlock = null;

            foreach (var reservation in reservations)
            {
                if (reservation.status == 'A')
                    continue;

                bool isVacation = (vacationService != null && reservation.ServiceId == vacationService.Id);

                if (isVacation)
                {
                    if (currentVacationBlock == null ||
                        reservation.time != currentVacationBlock.EndTime ||
                        reservation.status != currentVacationBlock.Status)
                    {
                        if (currentVacationBlock != null)
                            blocks.Add(currentVacationBlock);

                        string statusText = GetStatusText(reservation.status);

                        string modal = reservation.status switch
                        {
                            'O' => "#manageVacationModal",
                            'Z' => "",
                            _ => "#deleteReservationModal"
                        };

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
                        currentVacationBlock.EndTime = currentVacationBlock.EndTime.Add(TimeSpan.FromMinutes(15));
                    }
                }
                else
                {
                    bool startNew = false;
                    if (currentBlock == null)
                    {
                        startNew = true;
                    }
                    else
                    {
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

                        string modal = reservation.status switch
                        {
                            'O' => "#manageReservationModal",
                            'Z' => "",
                            _ => "#deleteReservationModal"
                        };

                        string clientInfo = (reservation.Client != null)
                            ? $"{reservation.Client.Name} {reservation.Client.Surname}\nTel: {reservation.Client.Phone}"
                            : "Zarezerwowane";

                        if (reservation.status == 'Z')
                        {
                            clientInfo += "\n(Zakoñczone)";
                        }

                        string blockClass = reservation.status switch
                        {
                            'O' => "pending",
                            'P' => "reserved",
                            'Z' => "completed",
                            _ => "reserved"
                        };

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
                            BlockClass = blockClass
                        };
                    }
                    else
                    {
                        currentBlock.EndTime = currentBlock.EndTime.Add(TimeSpan.FromMinutes(15));
                    }
                }
            }

            if (currentBlock != null)
                blocks.Add(currentBlock);
            if (currentVacationBlock != null)
                blocks.Add(currentVacationBlock);

            return blocks;
        }
        private void LoadVacationHistory(int hairdresserId)
        {
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

        public void HandleReservation(int reservationId)
        {
            OnPostDeleteReservation(reservationId);
        }

        public void HandleVacationRequest(DateTime date, TimeSpan startTime, TimeSpan endTime)
        {
            var request = new VacationRequest
            {
                startDate = date,
                endDate = date,
                startTime = startTime.ToString(),
                endTime = endTime.ToString(),
                type = "customPeriod"
            };

            OnPostVacationRequestAsync(request).Wait();
        }
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

                for (var day = startDate; day <= endDate; day = day.AddDays(1))
                {
                    var existing = await _context.Reservation
                        .Where(r => r.HairdresserId == hairdresserId
                                 && r.ServiceId == vacationService.Id
                                 && (r.status == 'O' || r.status == 'P')
                                 && r.date == day)
                        .ToListAsync();

                    if (!existing.Any())
                    {
                        var newSlots = CreateVacationSlots(day, st, et, systemClient.Id, hairdresserId.Value, vacationService.Id);
                        _context.Reservation.AddRange(newSlots);
                    }
                    else
                    {
                        var oldMin = existing.Min(r => r.time);
                        var oldMax = existing.Max(r => r.time).Add(TimeSpan.FromMinutes(15));

                        var oldRange = (start: oldMin, end: oldMax);
                        var newRange = (start: st, end: et);

                        bool newContainsOld = (newRange.start <= oldRange.start && newRange.end >= oldRange.end);
                        bool oldContainsNew = (oldRange.start <= newRange.start && oldRange.end >= newRange.end);
                        bool overlap = !(newRange.end <= oldRange.start || newRange.start >= oldRange.end);

                        if (newContainsOld)
                        {
                            if (existing.Any(r => r.status == 'P'))
                            {
                                return BadRequest($"Dnia {day:dd.MM.yyyy} istnieje ju¿ potwierdzony urlop.");
                            }
                            _context.Reservation.RemoveRange(existing);
                            var newSlots = CreateVacationSlots(day, newRange.start, newRange.end,
                                systemClient.Id, hairdresserId.Value, vacationService.Id);
                            _context.Reservation.AddRange(newSlots);
                        }
                        else if (oldContainsNew || overlap)
                        {
                            return BadRequest($"Dnia {day:dd.MM.yyyy} istnieje ju¿ urlop w tym czasie.");
                        }
                        else
                        {
                            var newSlots = CreateVacationSlots(day, newRange.start, newRange.end,
                                systemClient.Id, hairdresserId.Value, vacationService.Id);
                            _context.Reservation.AddRange(newSlots);
                        }
                    }
                }

                await _context.SaveChangesAsync();
                return new JsonResult(new
                {
                    success = true,
                    message = $"Urlop utworzony w zakresie {startDate:dd.MM.yyyy} - {endDate:dd.MM.yyyy}."
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
                    status = 'O',
                    ClientId = clientId,
                    HairdresserId = hairdresserId,
                    ServiceId = serviceId
                });
                current = current.Add(TimeSpan.FromMinutes(15));
            }
            return slots;
        }
        public IActionResult OnPostDeleteReservation(int reservationId)
        {
            var reservation = _context.Reservation.FirstOrDefault(r => r.Id == reservationId);
            if (reservation != null && (reservation.status == 'O' || reservation.status == 'P'))
            {
                reservation.status = 'A';
                _context.SaveChanges();
                var tempId = reservationId + 1;
                var next = _context.Reservation.FirstOrDefault(r => r.Id == tempId);
                while (next != null &&
                       next.ClientId == reservation.ClientId &&
                       next.date == reservation.date &&
                       next.ServiceId == reservation.ServiceId &&
                       (next.status == 'O' || next.status == 'P') &&
                       next.time == reservation.time.Add(TimeSpan.FromMinutes(15)))
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

                var tempId = reservationId + 1;
                var next = _context.Reservation.FirstOrDefault(r => r.Id == tempId);
                while (next != null &&
                       next.ClientId == reservation.ClientId &&
                       next.date == reservation.date &&
                       next.ServiceId == reservation.ServiceId &&
                       next.status == 'O' &&
                       next.time == reservation.time.Add(TimeSpan.FromMinutes(15)))
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

        public class VacationRequest
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