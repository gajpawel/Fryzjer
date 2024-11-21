using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Fryzjer.Pages.Hairdressers
{
    public class HairdresserProfileModel : PageModel
    {
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Status { get; set; }

        public void OnGet()
        {
            
        }
    }
}
