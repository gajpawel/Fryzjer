using System.ComponentModel.DataAnnotations;

namespace Fryzjer.Models
{
    public class Service // usługa
    {
        [Key]
        public int Id { get; set; }
        public string? Name { get; set; }
        public TimeSpan Duration { get; set; }
        public double price { get; set; }

    }
}
