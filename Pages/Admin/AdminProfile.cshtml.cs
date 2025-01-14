using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Fryzjer.Pages.Admin
{
    public class AdminProfile : PageModel //strona g³ówna konta admina
    {
        public IActionResult OnPostSchedule()
        {
            // Po klikniêciu przycisku "Przegl¹d harmonogramów"
            return RedirectToPage("/ScheduleManagement");
        }

        public IActionResult OnPostRequests()
        {
            // Po klikniêciu przycisku "Przegl¹d wniosków o urlop"
            return RedirectToPage("/Admin/Requests");
        }

        public IActionResult OnPostEmployeeManagement()
        {
            // Po klikniêciu przycisku "Zarz¹dzanie pracownikami"
            return RedirectToPage("/Admin/Employee/EmployeeManagement");
        }

        public IActionResult OnPostSalon()
        {
            // Po klikniêciu przycisku "Przegl¹d listy lokali"
            return RedirectToPage("/Admin/Salon/Salon");
        }

        public IActionResult OnPostServices()
        {
            // Po klikniêciu przycisku "Zarz¹dzanie us³ugami"
            return RedirectToPage("/Admin/Services/Services");
        }
        public IActionResult OnPostAccessManagement()
        {
            // Po klikniêciu przycisku "Dane logowania"
            return RedirectToPage("/Admin/AccessManagement");
        }
    }
}
