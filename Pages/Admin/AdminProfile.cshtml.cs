using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Fryzjer.Pages.Admin
{
    public class AdminProfile : PageModel
    {
        public IActionResult OnPostSchedule()
        {
            // Po klikniêciu przycisku "Przegl¹d harmonogramów"
            return RedirectToPage("/Admin/Schedule");
        }

        public IActionResult OnPostRequests()
        {
            // Po klikniêciu przycisku "Przegl¹d wniosków o urlop"
            return RedirectToPage("/Admin/Requests");
        }

        public IActionResult OnPostEmployeeManagement()
        {
            // Po klikniêciu przycisku "Zarz¹dzanie pracownikami"
            return RedirectToPage("/Admin/EmployeeManagement");
        }

        public IActionResult OnPostSalon()
        {
            // Po klikniêciu przycisku "Przegl¹d listy lokali"
            return RedirectToPage("/Admin/Salon");
        }

        public IActionResult OnPostServices()
        {
            // Po klikniêciu przycisku "Zarz¹dzanie us³ugami"
            return RedirectToPage("/Admin/Services/Services");
        }
    }
}
