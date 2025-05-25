using Microsoft.EntityFrameworkCore;
using SkillSyncAPI.Data;
using SkillSyncAPI.Domain.Entities;

namespace SkillSyncAPI.Repositories.Impl
{
    public class UserRepository : IUserRepository
    {

        private readonly ApplicationDbContext _context;

        public UserRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public IEnumerable<ApplicationUser> GetAll() => _context.Users.AsNoTracking().ToList();

        public ApplicationUser? GetById(int id) => _context.Users.Find(id);

        public void Add(ApplicationUser user) => _context.Users.Add(user);

        public void Update(ApplicationUser user) => _context.Users.Update(user);

        public void Delete(ApplicationUser user) => _context.Users.Remove(user);

        public async Task SaveChangesAsync() => await _context.SaveChangesAsync();

    }
}
