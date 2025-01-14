using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Fryzjer.Pages.Admin
{
    public class AdminProfile : PageModel //strona g��wna konta admina
    {
        public IActionResult OnPostSchedule()
        {
            // Po klikni�ciu przycisku "Przegl�d harmonogram�w"
            return RedirectToPage("/ScheduleManagement");
        }

        public IActionResult OnPostRequests()
        {
            // Po klikni�ciu przycisku "Przegl�d wniosk�w o urlop"
            return RedirectToPage("/Admin/Requests");
        }

        public IActionResult OnPostEmployeeManagement()
        {
            // Po klikni�ciu przycisku "Zarz�dzanie pracownikami"
            return RedirectToPage("/Admin/Employee/EmployeeManagement");
        }

        public IActionResult OnPostSalon()
        {
            // Po klikni�ciu przycisku "Przegl�d listy lokali"
            return RedirectToPage("/Admin/Salon/Salon");
        }

        public IActionResult OnPostServices()
        {
            // Po klikni�ciu przycisku "Zarz�dzanie us�ugami"
            return RedirectToPage("/Admin/Services/Services");
        }
        public IActionResult OnPostAccessManagement()
        {
            // Po klikni�ciu przycisku "Dane logowania"
            return RedirectToPage("/Admin/AccessManagement");
        }
    }
}
