using SkillSyncAPI.Data;
using SkillSyncAPI.Domain.Entities;

namespace SkillSyncAPI.Repositories.Impl
{
    public class ServiceRepository : IServiceRepository
    {
        private readonly ApplicationDbContext _context;

        public ServiceRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public IQueryable<Service> Query() => _context.Services.AsQueryable();

        public Service? GetById(int id) => _context.Services.Find(id);

        public void Add(Service service) => _context.Services.Add(service);

        public void Update(Service service) => _context.Services.Update(service);

        public void Delete(Service service) => _context.Services.Remove(service);

        public async Task SaveChangesAsync() => await _context.SaveChangesAsync();
    }
}
