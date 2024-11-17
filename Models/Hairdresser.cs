using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Fryzjer.Models
{
    public class Hairdresser
    {
        [Key]
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? surname { get; set; }
        public string? login { get; set; } // zadbać by był niepowtarzalny
        public string? password { get; set; } // nałożyć wymagania silnego hasła
        public string? photoPath { get; set; } //ścieżka do zdjęcia profilowego
        public char status { get; set; } // A - pracuje, X - zwolniony, T - tymczasowo bez przypisanego lokalu
        
        [ForeignKey("Place")]
        public int? PlaceId { get; set; } //ID lokalu, w którym pracuje
        public Place? Place { get; set; }

    }
}
