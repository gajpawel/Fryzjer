using Fryzjer.Data;
using Microsoft.EntityFrameworkCore;

namespace Fryzjer.Repositories
{
    public class SpecializationRepository: ISpecializationRepository
    {
        private readonly FryzjerContext _context;

        public SpecializationRepository(FryzjerContext context)
        {
            _context = context;
        }
        public Models.Specialization? getById(int id)
        {
            return _context.Specialization.FirstOrDefault(h => h.Id == id);
        }

        public List<Models.Specialization> getByHairdresserId(int hairdresserId)
        {
            return _context.Specialization
                .Where(s => s.HairdresserId == hairdresserId)
                .ToList();
        }
        public Models.Specialization? getByName(string name)
        {
            return _context.Specialization.FirstOrDefault(h => h.Name == name);
        }
        public List<Models.Specialization> getAll()
        {
            return _context.Specialization.ToList();
        }
        public void deleteById(int id)
        {
            var specialization = _context.Specialization.Find(id);
            if (specialization == null)
            {
                return;
            }
            _context.Specialization.Remove(specialization);
        }
        public void insert(Models.Specialization entity)
        {
            if (entity != null)
                _context.Add(entity);
        }
        public void update(Models.Specialization entity)
        {
            if (entity != null)
                _context.Update(entity);
        }
        public void deleteAll()
        {
            _context.Remove(_context.Specialization);
        }
        public void save()
        {
            _context.SaveChanges();
        }
    }
}
