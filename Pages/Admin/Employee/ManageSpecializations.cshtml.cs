using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Fryzjer.Data;
using Fryzjer.Models;
using Fryzjer.Repositories;

namespace Fryzjer.Pages.Admin
{
    public class ManageSpecializationsModel : PageModel
    {
        private SpecializationRepository _specializationRepository;
        private HairdresserRepository _hairdresserRepository;
        private ServiceRepository _serviceRepository;

        public ManageSpecializationsModel(FryzjerContext context)
        {
            _specializationRepository = new SpecializationRepository(context);
            _hairdresserRepository = new HairdresserRepository(context);
            _serviceRepository = new ServiceRepository(context);
        }

        [BindProperty]
        public int HairdresserId { get; set; }

        public Hairdresser? Hairdresser { get; set; }
        public List<Service> AvailableServices { get; set; } = new();
        public List<Specialization> CurrentSpecializations { get; set; } = new();
        [BindProperty]
        public List<int> SelectedServices { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int hairdresserId)
        {
            HairdresserId = hairdresserId;

            Hairdresser = _hairdresserRepository.getById(hairdresserId);

            if (Hairdresser == null)
            {
                return NotFound();
            }

            AvailableServices = _serviceRepository.getAll();
            CurrentSpecializations = _specializationRepository.getByHairdresserId(hairdresserId);

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Usuñ wszystkie obecne specjalizacje
            var currentSpecs = _specializationRepository.getByHairdresserId(HairdresserId);
            foreach (var spec in currentSpecs)
                _specializationRepository.deleteById(spec.Id);

            // Dodaj nowe specjalizacje
            foreach (var serviceId in SelectedServices)
            {
                Specialization s = new Specialization
                {
                    HairdresserId = HairdresserId,
                    ServiceId = serviceId
                };
                _specializationRepository.insert(s);
            }

            _specializationRepository.save();

            return RedirectToPage("/Admin/Employee/EmployeeManagement");
        }
    }
}