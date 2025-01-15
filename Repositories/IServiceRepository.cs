namespace Fryzjer.Repositories
{
    public interface IServiceRepository
    {
        Models.Service? getById(int id);
        Models.Service? getByName(string name);
        List<Models.Service> getAll();
        void deleteById(int id);
        void insert(Models.Service entity);
        void update(Models.Service entity);
        void deleteAll();
        void save();
    }
}
