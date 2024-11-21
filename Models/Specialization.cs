using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Fryzjer.Models
{
    public class Specialization // przypisanie fryzjera do usługi - np. fryzjer A może farbować i podcinać koncówki, a fryzjer B może strzyc brodę i farbować
    {
        [Key]
        public int Id { get; set; }
        public string? Name { get; set; }
        
        [ForeignKey("Hairdresser")]
        public int HairdresserId { get; set; }
        public Hairdresser? Hairdresser { get; set; }
        
        [ForeignKey("Service")]
        public int ServiceId { get; set; }
        public Service? Service { get; set; }
    }
}
