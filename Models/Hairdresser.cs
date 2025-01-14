using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Fryzjer.Models
{
    public class Hairdresser
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Imię jest wymagane")]
        public string? Name { get; set; }

        [Required(ErrorMessage = "Nazwisko jest wymagane")]
        public string? Surname { get; set; }

        [Required(ErrorMessage = "Login jest wymagany")]
        public string? login { get; set; }

        public string? password { get; set; }

        public string? photoPath { get; set; } //ścieżka do zdjęcia profilowego
        public string? description { get; set; } //treść wizytówki

        [ForeignKey("Place")]
        public int? PlaceId { get; set; }

        public Place? Place { get; set; }
    }
}