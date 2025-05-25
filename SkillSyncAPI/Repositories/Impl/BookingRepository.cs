using SkillSyncAPI.Data;
using SkillSyncAPI.Domain.Entities;

namespace SkillSyncAPI.Repositories.Impl
{
    public class BookingRepository : IBookingRepository
    {
        private readonly ApplicationDbContext _context;

        public BookingRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public IQueryable<Booking> Query() => _context.Bookings;

        public Booking? GetById(int id) => _context.Bookings.Find(id);

        public void Add(Booking booking) => _context.Bookings.Add(booking);

        public void Update(Booking booking) => _context.Bookings.Update(booking);

        public void Delete(Booking booking) => _context.Bookings.Remove(booking);

        public Task<int> SaveChangesAsync() => _context.SaveChangesAsync();
    }
}
