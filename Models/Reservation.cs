using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Fryzjer.Models
{
    public class Reservation
    {
        [Key]
        public int Id { get; set; }
        public DateTime date { get; set; }
        public TimeSpan time { get; set; }
        public char status { get; set; } // O - oczekuje na akceptację, P - potwierdzona, Z - zakończona A - anulowana

        [ForeignKey("Client")]
        public int? ClientId { get; set; }
        public Client? Client { get; set; }

        [ForeignKey("Hairdresser")]
        public int? HairdresserId { get; set; }
        public Hairdresser? Hairdresser { get; set; }

        [ForeignKey("Service")]
        public int? ServiceId { get; set; }
        public Service? Service { get; set; }
    }
}
