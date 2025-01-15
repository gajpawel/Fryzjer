using Fryzjer.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Fryzjer.Repositories
{
    public class HairdresserRepository: IHairdresserRepository
    {
        private readonly FryzjerContext _context;

        public HairdresserRepository(FryzjerContext context)
        {
            _context = context;
        }
        public Models.Hairdresser? getById(int id)
        {
            return _context.Hairdresser.FirstOrDefault(h => h.Id == id);
        }
        public Models.Hairdresser? getByName(string name)
        {
            return _context.Hairdresser.FirstOrDefault(h => h.Name == name);
        }

        public Models.Hairdresser? getAndIncludePlace(int id)
        {
           return _context.Hairdresser
                .Include(h => h.Place) // Jeśli chcesz również dane lokalu
                .FirstOrDefault(m => m.Id == id);
        }
        public List<Models.Hairdresser> getAll()
        {
            return _context.Hairdresser.ToList();
        }
        public void deleteById(int id)
        {
            var hairdresser = _context.Hairdresser.Find(id);
            if (hairdresser == null)
            {
                return;
            }
            _context.Hairdresser.Remove(hairdresser);
        }
        public void insert(Models.Hairdresser entity)
        {
            if (entity != null)
                _context.Add(entity);
        }
        public void update(Models.Hairdresser entity)
        {
            if (entity != null)
                _context.Update(entity);
        }
        public void deleteAll()
        {
            _context.Remove(_context.Hairdresser);
        }

        public bool exists(int id)
        {
            return _context.Hairdresser.Any(h => h.Id == id);
        }
        public void save()
        {
            _context.SaveChanges();
        }
    }
}
