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

        [CustomValidation(typeof(Hairdresser), nameof(ValidatePassword))]
        public string? password { get; set; }

        public string? photoPath { get; set; }

        [ForeignKey("Place")]
        public int? PlaceId { get; set; }

        public Place? Place { get; set; }

        public static ValidationResult ValidatePassword(string? password, ValidationContext context)
        {
            if (string.IsNullOrEmpty(password))
                return ValidationResult.Success; // puste hasło jest ok (nie zmieniamy hasła)

            if (!password.Any(char.IsUpper) || !password.Any(char.IsLower))
                return new ValidationResult("Hasło musi zawierać co najmniej jedną wielką i jedną małą literę.");
            return ValidationResult.Success;
        }
    }
}