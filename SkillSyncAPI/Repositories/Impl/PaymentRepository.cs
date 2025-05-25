using SkillSyncAPI.Data;
using SkillSyncAPI.Domain.Entities;

namespace SkillSyncAPI.Repositories.Impl
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly ApplicationDbContext _context;

        public PaymentRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public IQueryable<Payment> Query() => _context.Payments;

        public Payment? GetById(int id) => _context.Payments.Find(id);

        public void Add(Payment payment) => _context.Payments.Add(payment);

        public void Update(Payment payment) => _context.Payments.Update(payment);

        public async Task<int> SaveChangesAsync() => await _context.SaveChangesAsync();
    }
}
