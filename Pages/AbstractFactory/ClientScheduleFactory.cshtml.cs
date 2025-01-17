using Fryzjer.Data;
using Fryzjer.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
namespace Fryzjer.Pages.AbstractFactory
{
    // Konkretna implementacja fabryki dla harmonogramu klienta
    public class ClientScheduleFactory : ScheduleFactory
    {
        private readonly FryzjerContext _context;
        public ClientScheduleFactory(PageModel pageModel, FryzjerContext context) : base(pageModel)
        {
            _context = context;
        }
        public override IScheduleOperations CreateSchedule()
        {
            return new ClientScheduleOperations(_context);
        }
    }


    // Konkretna implementacja operacji na harmonogramie klienta
    public class ClientScheduleOperations : IScheduleOperations
    {
        private readonly FryzjerContext _context;
        public ClientScheduleOperations(FryzjerContext context)
        {
            _context = context;
        }
        public (List<DailySchedule>, List<DailySchedule>) CreateSchedule(int hairdresserId, DateTime startDate)
        {
            var weeklySchedule1 = GenerateScheduleInternalAsync(startDate, hairdresserId).Result; // Wywo³anie wewnêtrznej metody
            var weeklySchedule2 = GenerateScheduleInternalAsync(startDate.AddDays(7), hairdresserId).Result;

            return (weeklySchedule1, weeklySchedule2);
        }

        public void HandleReservation(int reservationId)
        {
            throw new NotImplementedException();
        }
        public void HandleVacationRequest(DateTime date, TimeSpan startTime, TimeSpan endTime)
        {
            // Klient nie powinien móc dodawaæ urlopów, wiêc mo¿na zostawiæ puste lub rzuciæ wyj¹tek
            throw new NotImplementedException("Clients cannot handle vacation requests");
        }



        private async Task<List<DailySchedule>> GenerateScheduleInternalAsync(DateTime startDate, int hairdresserId)
        {
            var schedule = new List<DailySchedule>();

            for (int i = 0; i < 5; i++)
            {
                var date = startDate.AddDays(i);
                var timeBlocks = new List<TimeBlock>();

                if (date >= DateTime.Now.Date)
                {
                    timeBlocks = await GenerateDailyTimeBlocksAsync(date, hairdresserId);
                }

                schedule.Add(new DailySchedule
                {
                    Date = date,
                    TimeBlocks = timeBlocks
                });
            }

            return schedule;
        }


        public async Task<List<DailySchedule>> GenerateScheduleAsync(DateTime startDate, int hairdresserId)
        {

            var schedule = new List<DailySchedule>();

            for (int i = 0; i < 5; i++)
            {
                var date = startDate.AddDays(i);
                var timeBlocks = new List<TimeBlock>();

                if (date >= DateTime.Now.Date)
                {
                    timeBlocks = await GenerateDailyTimeBlocksAsync(date, hairdresserId);
                }

                schedule.Add(new DailySchedule
                {
                    Date = date,
                    TimeBlocks = timeBlocks
                });
            }

            return schedule;
        }

        public async Task<List<TimeBlock>> GenerateDailyTimeBlocksAsync(DateTime date, int hairdresserId)
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
                    currentBlock = new TimeBlock
                    {
                        StartTime = reservation.time,
                        EndTime = reservation.time.Add(new TimeSpan(0, 15, 0)),
                        IsReserved = true,
                        ReservationId = reservation.Id,
                        ClientInfo = "Zarezerwowane",
                    
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


}
