using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System;
using Fryzjer.Models;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Fryzjer.Pages.AbstractFactory
{

    /// Abstrakcyjna klasa bazowa dla wszystkich fabryk harmonogramów

    public abstract class ScheduleFactoryModel : PageModel
    {
        // W?a?ciwo?ci wspólne dla wszystkich harmonogramów
        public List<DailySchedule> WeeklySchedule1 { get; set; } = new List<DailySchedule>();
        public List<DailySchedule> WeeklySchedule2 { get; set; } = new List<DailySchedule>();
        public int CurrentWeek { get; set; } = 0;
        public List<Service> Services { get; set; } = new List<Service>();

        // Metoda abstrakcyjna do generowania harmonogramu - ka?da klasa pochodna musi j? zaimplementowa?
        public abstract void OnGet(int week = 0);
    }

    /// Interfejs definiuj?cy podstawowe operacje na harmonogramie
    public interface IScheduleOperations
    {
        (List<DailySchedule>, List<DailySchedule>) CreateSchedule(int hairdresserId, DateTime startDate);
        void HandleReservation(int reservationId);
        void HandleVacationRequest(DateTime date, TimeSpan startTime, TimeSpan endTime);
    }


    /// Klasa reprezentuj?ca harmonogram dnia

    public class DailySchedule
    {
        public DateTime Date { get; set; }
        public List<TimeBlock> TimeBlocks { get; set; } = new List<TimeBlock>();
    }

    /// Klasa reprezentuj?ca blok czasowy w harmonogramie
    public class TimeBlock
    {
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public bool IsReserved { get; set; }
        public string TimeRange => $"{StartTime:hh\\:mm} - {EndTime:hh\\:mm}";
        public string? ClientInfo { get; set; }
        public int? ReservationId { get; set; }
        public int? ServiceId { get; set; }
        public string? ServiceName { get; set; }
        public string? BlockClass { get; set; }
        public string? Modal { get; set; }
        public char Status { get; set; }
        public int ClientId { get; set; }
    }

    /// Fabryka abstrakcyjna dla harmonogramów

    public abstract class ScheduleFactory
    {
        protected readonly PageModel _pageModel;

        protected ScheduleFactory(PageModel pageModel)
        {
            _pageModel = pageModel;
        }

        public abstract IScheduleOperations CreateSchedule();
    }
}