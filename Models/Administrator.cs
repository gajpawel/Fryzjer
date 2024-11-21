using System.ComponentModel.DataAnnotations;

namespace Fryzjer.Models
{
    public class Administrator //klasa typu singleton (chyba że chcemy więcej administratorów)
    {
        [Key]
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Surname { get; set; }
        public string? Login { get; set; }
        public string? Password { get; set; }
    }
}
