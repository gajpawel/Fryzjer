using System.ComponentModel.DataAnnotations;

namespace Fryzjer.Models;
public class Client
{
    [Key]
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Surname { get; set; }
    public char Gender { get; set; } // M - mężczyzna, K - kobieta, null - inne
    public string? Login { get; set; } // zadbać, by był niepowtarzalny
    public string? Password { get; set; } // nałożyć wymagania silnego hasła
    public string? Phone { get; set; } // numer telefonu
}
