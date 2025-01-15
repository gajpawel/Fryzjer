using Fryzjer.Data;
using Microsoft.EntityFrameworkCore;

namespace Fryzjer.Repositories
{
    public class PlaceRepository: IPlaceRepository
    {
        private readonly FryzjerContext _context;

        public PlaceRepository(FryzjerContext context)
        {
            _context = context;
        }
        public Models.Place? getById(int id)
        {
            return _context.Place.FirstOrDefault(h => h.Id == id);
        }
        public Models.Place? getByName(string name)
        {
            return _context.Place.FirstOrDefault(h => h.Name == name);
        }
        public List<Models.Place> getAll()
        {
           return _context.Place.ToList();
        }
        public void deleteById(int id)
        {
            var salon = _context.Place.Find(id);
            if (salon == null)
            {
                return;
            }
            _context.Place.Remove(salon);
        }
        public void insert(Models.Place entity)
        {
            if(entity != null)
                _context.Add(entity);
        }
        public void update(Models.Place entity)
        {
            if(entity != null)
                _context.Update(entity);
        }
        public void deleteAll()
        {
            _context.Remove(_context.Place);
        }
        public void save()
        {
            _context.SaveChanges();
        }
    }
}
