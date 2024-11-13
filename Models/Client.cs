using System.ComponentModel.DataAnnotations;

namespace Fryzjer.Models;
public class Client
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Surname { get; set; }
    public char Gender { get; set; }
    public string? Login { get; set; }
    public string? Password { get; set; }
    public string? Phone { get; set; }
}
