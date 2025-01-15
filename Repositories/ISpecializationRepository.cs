namespace Fryzjer.Repositories
{
    public interface ISpecializationRepository
    {
        Models.Specialization? getById(int id);
        Models.Specialization? getByName(string name);
        List<Models.Specialization> getAll();
        void deleteById(int id);
        void insert(Models.Specialization entity);
        void update(Models.Specialization entity);
        void deleteAll();
        void save();
    }
}
