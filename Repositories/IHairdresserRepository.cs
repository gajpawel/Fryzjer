namespace Fryzjer.Repositories
{
    public interface IHairdresserRepository
    {
        Models.Hairdresser? getById(int id);
        Models.Hairdresser? getByName(string name);
        List<Models.Hairdresser> getAll();
        void deleteById(int id);
        void insert(Models.Hairdresser entity);
        void update(Models.Hairdresser entity);
        void deleteAll();
        void save();
    }
}
