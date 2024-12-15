using System.ComponentModel.DataAnnotations;

namespace Fryzjer.Models
{
    public class Service // usługa
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Nazwa usługi jest wymagana.")]
        public string? Name { get; set; }

        [Required(ErrorMessage = "Czas trwania jest wymagany.")]
        [RegularExpression(@"^\d{2}:\d{2}:\d{2}$", ErrorMessage = "Czas trwania musi być w formacie hh:mm:ss.")]
        public TimeSpan Duration { get; set; }

        [Required(ErrorMessage = "Cena jest wymagana.")]
        [Range(0, double.MaxValue, ErrorMessage = "Cena musi być większa lub równa 0.")]
        public double Price { get; set; }
    }
}
