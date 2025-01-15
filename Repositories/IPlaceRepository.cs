namespace Fryzjer.Repositories
{
    public interface IPlaceRepository
    {
        Models.Place? getById(int id);
        Models.Place? getByName(string name);
        List<Models.Place> getAll();
        void deleteById(int id);
        void insert(Models.Place entity);
        void update(Models.Place entity);
        void deleteAll();
        void save();
    }
}
