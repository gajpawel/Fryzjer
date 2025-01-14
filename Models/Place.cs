using System.ComponentModel.DataAnnotations;

namespace Fryzjer.Models
{
    public class Place //lokal, salon fryzjerski
    {
        [Key]
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? address { get; set; }
        public string? logoPath { get; set; } //ścieżka do logo lokalu
        public string? photoPath { get; set; } //ścieżka do zdjęcia lokalu
        public string? description { get; set; } //opis (wizytówka) lokalu
        public string? telephoneNumber { get; set; }
    }
}
