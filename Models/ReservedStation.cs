using System.ComponentModel.DataAnnotations;

using System.ComponentModel.DataAnnotations.Schema;

namespace Fryzjer.Models
{
    public class ReservedStation // w schemacie SQL Developer tabela "Zajmuje"  - informuje z jakich stanowisk korzysta rezerwacja (może być więcej niż jedno)
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Reservation")]
        public int ReservationId { get; set; }
        public Reservation? Reservation { get; set; }

        [ForeignKey("Station")]
        public int StationId { get; set; }
        public Station? Station { get; set; }
    }
}
