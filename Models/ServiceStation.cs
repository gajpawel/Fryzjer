using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Fryzjer.Models
{
    public class ServiceStation // w SQL Developer tabela o nazwie "Korzysta" - informuje, jakiego typu stanowisk wymaga dana usługa (np. farbowanie wymaga st. do farbowania i do płukania)
    {
        [Key]
        public int Id { get; set; }
        
        [ForeignKey("Service")]
        public int ServiceId { get; set; }
        public Service? Service { get; set; }
        
        [ForeignKey("StationType")]
        public int StationTypeId { get; set; }
        public StationType? StationType { get; set; }
    }
}
