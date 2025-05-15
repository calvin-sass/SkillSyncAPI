using Microsoft.EntityFrameworkCore;
using SkillSyncAPI.Helpers.Interfaces;
using SkillSyncAPI.Models;

namespace SkillSyncAPI.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }

        public DbSet<Service> Services { get; set; }

        public DbSet<Booking> Bookings { get; set; }

        public DbSet<Review> Reviews { get; set; }

        public DbSet<Notification> Notifications { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User
            modelBuilder.Entity<User>(user =>
            {
                user.HasMany(u => u.Services)
                    .WithOne(s => s.User)
                    .HasForeignKey(s => s.UserId);

                user.HasQueryFilter(u => !u.IsDeleted);

                user.HasIndex(u => u.Username)
                    .IsUnique();
            });

            // SERVICE  
            modelBuilder.Entity<Service>(service =>
            {
                service.HasMany(s => s.Bookings)
                       .WithOne(b => b.Service)
                       .HasForeignKey(b => b.ServiceId);

                service.HasMany(s => s.Reviews)
                       .WithOne(r => r.Service)
                       .HasForeignKey(r => r.ServiceId);
            });

            // BOOKING
            modelBuilder.Entity<Booking>(booking =>
            {
                booking.HasOne(b => b.User)
                       .WithMany(u => u.Bookings)
                       .HasForeignKey(b => b.UserId)
                       .OnDelete(DeleteBehavior.Restrict);

                booking.HasOne(b => b.Service)
                       .WithMany(s => s.Bookings)
                       .HasForeignKey(b => b.ServiceId)
                       .OnDelete(DeleteBehavior.Cascade);

                booking.Property(b => b.ModifiedByRole)
                       .HasMaxLength(10); // To limit values like "User" or "Seller"
            });

            // PAYMENT
            modelBuilder.Entity<Payment>(payment =>
            {
                payment.HasOne(p => p.Booking)
                       .WithOne(b => b.Payment)
                       .HasForeignKey<Payment>(p => p.BookingId)
                       .OnDelete(DeleteBehavior.Cascade);
            });

            // REVIEW
            modelBuilder.Entity<Review>(review =>
            {
                review.HasOne(r => r.User)
                      .WithMany(u => u.Reviews)
                      .HasForeignKey(r => r.UserId)
                      .OnDelete(DeleteBehavior.Restrict);

                review.HasOne(r => r.Service)
                      .WithMany(s => s.Reviews)
                      .HasForeignKey(r => r.ServiceId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // NOTIFICATION
            modelBuilder.Entity<Notification>(notification =>
            {
                notification.HasOne(n => n.User)
                            .WithMany(u => u.Notifications) // Make sure User has ICollection<Notification>
                            .HasForeignKey(n => n.UserId)
                            .OnDelete(DeleteBehavior.Cascade);
            });        
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var entries = ChangeTracker
                .Entries<IAuditable>();

            var utcNow = DateTime.UtcNow;

            foreach (var entry in entries)
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreatedAt = utcNow;
                    entry.Entity.UpdatedAt = utcNow;
                }
                else if (entry.State == EntityState.Modified)
                {
                    entry.Entity.UpdatedAt = utcNow;
                }
            }

            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}
