using Fryzjer.Data;

namespace Fryzjer.Repositories
{
    public class ServiceRepository: IServiceRepository
    {
        private readonly FryzjerContext _context;

        public ServiceRepository(FryzjerContext context)
        {
            _context = context;
        }
        public Models.Service? getById(int id)
        {
            return _context.Service.FirstOrDefault(h => h.Id == id);
        }
        public Models.Service? getByName(string name)
        {
            return _context.Service.FirstOrDefault(h => h.Name == name);
        }
        public List<Models.Service> getAll()
        {
            return _context.Service.ToList();
        }
        public void deleteById(int id)
        {
            var service = _context.Service.Find(id);
            if (service == null)
            {
                return;
            }
            _context.Service.Remove(service);
        }
        public void insert(Models.Service entity)
        {
            if (entity != null)
                _context.Add(entity);
        }
        public void update(Models.Service entity)
        {
            if (entity != null)
                _context.Update(entity);
        }
        public void deleteAll()
        {
            _context.Remove(_context.Service);
        }
        public void save()
        {
            _context.SaveChanges();
        }
    }
}
