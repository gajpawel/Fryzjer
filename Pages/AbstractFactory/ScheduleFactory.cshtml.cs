using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;

namespace Fryzjer.Pages.AbstractFactory
{
    public abstract class ScheduleFactoryModel : PageModel
    {
        // W�a�ciwo�ci wsp�lne dla wszystkich harmonogram�w
        public List<DailySchedule> WeeklySchedule { get; set; } = new List<DailySchedule>();
        public int CurrentWeek { get; set; } = 0;

        // Metoda abstrakcyjna do generowania harmonogramu
        public abstract void GenerateSchedule(int week);
    }

    // Klasa pomocnicza - Harmonogram dnia
    public class DailySchedule
    {
        public DateTime Date { get; set; } // Data dnia
        public List<TimeBlock> TimeBlocks { get; set; } = new List<TimeBlock>(); // Lista blok�w czasowych
    }

    // Klasa pomocnicza - Blok czasowy
    public class TimeBlock
    {
        public TimeSpan StartTime { get; set; } // Godzina rozpocz�cia
        public TimeSpan EndTime { get; set; } // Godzina zako�czenia
        public bool IsReserved { get; set; } // Czy blok jest zarezerwowany
        public string TimeRange => $"{StartTime:hh\\:mm} - {EndTime:hh\\:mm}"; // Zakres czasowy
        public string? ClientInfo { get; set; } // Informacje o kliencie (je�li godzina jest zarezerwowana)
        public int? ReservationId { get; set; } // Identyfikator rezerwacji (je�li istnieje)

        public int? ServiceId { get; set; } // Identyfikator us�ugi
        public string? ServiceName { get; set; } // Nazwa us�ugi
    }

}
