using System.ComponentModel.DataAnnotations;

namespace Fryzjer.Models
{
    public class StationType //opisuje typ stanowisk - np. stanowisko do płukania
    {
        [Key]
        public int Id { get; set; }
        public string? description { get; set; } //np. stanowisko do farbowania, do cięcia, do suszenia
    }
}
