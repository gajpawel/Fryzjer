using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Fryzjer.Models
{
    public class Station // opisuje konkretne stanowisko - np. pierwsze stanowisko do płukania w lokalu X
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("StationType")]
        public int StationTypeId { get; set; }
        public StationType? StationType { get; set; }

        [ForeignKey("Place")]
        public int PlaceId { get; set; }
        public Place? Place { get; set; }
    }
}
