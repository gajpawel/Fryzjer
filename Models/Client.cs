using System.ComponentModel.DataAnnotations;

namespace Fryzjer.Models
{
    public class Client
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Imię jest wymagane")]
        [RegularExpression(@"^[A-ZĄĆĘŁŃÓŚŹŻ][a-ząćęłńóśźż]*$", ErrorMessage = "Imię musi zaczynać się od wielkiej litery.")]
        public string? Name { get; set; }

        [Required(ErrorMessage = "Nazwisko jest wymagane")]
        [RegularExpression(@"^[A-ZĄĆĘŁŃÓŚŹŻ][a-ząćęłńóśźż]*$", ErrorMessage = "Nazwisko musi zaczynać się od wielkiej litery.")]
        public string? Surname { get; set; }

        [Required(ErrorMessage = "Płeć jest wymagana")]
        public char Gender { get; set; } // 'M' - mężczyzna, 'K' - kobieta, 'N' - inne

        [Required(ErrorMessage = "Login jest wymagany")]
        [StringLength(50, ErrorMessage = "Login nie może przekraczać 50 znaków")]
        public string? Login { get; set; }

        [Required(ErrorMessage = "Hasło jest wymagane")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z]).{6,}$", ErrorMessage = "Hasło musi zawierać co najmniej jedną wielką literę, jedną małą literę i co najmniej 6 znaków.")]
        public string? Password { get; set; }

        [Required(ErrorMessage = "Numer telefonu jest wymagany")]
        [RegularExpression(@"^\d{9}$", ErrorMessage = "Numer telefonu musi składać się z dokładnie 9 cyfr.")]
        public string? Phone { get; set; }
    }
}
