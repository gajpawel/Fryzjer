// ScheduleFactory.cshtml.cs
using Microsoft.AspNetCore.Mvc.RazorPages;
using Fryzjer.Data;
using Fryzjer.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Fryzjer.Pages.AbstractFactory
{
    public abstract class ScheduleFactoryModel : PageModel
    {
        public List<DailySchedule> WeeklySchedule1 { get; set; } = new();
        public List<DailySchedule> WeeklySchedule2 { get; set; } = new();
        public int CurrentWeek { get; set; } = 0;
        public List<Service> Services { get; set; } = new();

        public abstract void OnGet(int week = 0);
    }

    public interface IScheduleOperations
    {
        (List<DailySchedule>, List<DailySchedule>) CreateSchedule(int hairdresserId, DateTime startDate);
        void HandleReservation(int reservationId);
        void HandleVacationRequest(DateTime date, TimeSpan startTime, TimeSpan endTime);
    }

    public class DailySchedule
    {
        public DateTime Date { get; set; }
        public List<TimeBlock> TimeBlocks { get; set; } = new();
    }

    public class TimeBlock
    {
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public bool IsReserved { get; set; }
        public string? ClientInfo { get; set; }
        public int? ReservationId { get; set; }
        public int? ServiceId { get; set; }
        public string? ServiceName { get; set; }
        public string? BlockClass { get; set; }
        public string? Modal { get; set; }
        public char Status { get; set; }
        public int ClientId { get; set; }
        public string TimeRange => $"{StartTime:hh\\:mm} - {EndTime:hh\\:mm}";
    }

    public abstract class ScheduleFactory
    {
        protected readonly PageModel PageModel;

        protected ScheduleFactory(PageModel pageModel)
        {
            PageModel = pageModel;
        }

        public abstract IScheduleOperations CreateSchedule();
    }
}
