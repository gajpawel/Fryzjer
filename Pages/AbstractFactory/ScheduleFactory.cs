using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System;
using Fryzjer.Models;
using Microsoft.EntityFrameworkCore.ChangeTracking;
namespace Fryzjer.Pages.AbstractFactory
{
    /// Interfejs definiujący podstawowe operacje na harmonogramie.
    public interface IScheduleOperations
    {
        (List<DailySchedule>, List<DailySchedule>) CreateSchedule(int hairdresserId, DateTime startDate);
        void HandleReservation(int reservationId);
        void HandleVacationRequest(DateTime date, TimeSpan startTime, TimeSpan endTime);
    }

    /// Klasa reprezentująca harmonogram (plan) pojedynczego dnia.
    public class DailySchedule
    {
        public DateTime Date { get; set; }
        public List<TimeBlock> TimeBlocks { get; set; } = new List<TimeBlock>();
    }
    /// Klasa reprezentująca blok czasowy w harmonogramie (np. 15-minutowy przedział).
    /// Zawiera informacje o rezerwacji, kliencie, usłudze itp.
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
    /// Abstrakcyjna fabryka dla harmonogramów (Abstract Factory).
    /// Zawiera metodę CreateSchedule() zwracającą obiekt IScheduleOperations.
    public abstract class ScheduleFactory
    {
        protected readonly PageModel _pageModel;
        protected ScheduleFactory(PageModel pageModel)
        {
            _pageModel = pageModel;
        }
        // Każda konkretna fabryka musi zaimplementować metodę
        // tworzącą/zwracającą obiekt IScheduleOperations
        public abstract IScheduleOperations CreateSchedule();
    }
}