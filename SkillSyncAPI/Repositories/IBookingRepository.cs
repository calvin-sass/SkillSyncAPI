using SkillSyncAPI.Domain.Entities;

namespace SkillSyncAPI.Repositories
{
    public interface IBookingRepository
    {
        IQueryable<Booking> Query();

        Booking? GetById(int id);

        void Add(Booking booking);

        void Update(Booking booking);

        void Delete(Booking booking);

        Task<int> SaveChangesAsync();
    }
}
