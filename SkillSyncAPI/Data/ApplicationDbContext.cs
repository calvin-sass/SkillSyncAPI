using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SkillSyncAPI.Domain.Entities;

namespace SkillSyncAPI.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole<int>, int>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<ApplicationUser> Users { get; set; }

        public DbSet<PendingUser> PendingUsers { get; set; }

        public DbSet<Service> Services { get; set; }

        public DbSet<ServiceImage> ServiceImages { get; set; }

        public DbSet<Booking> Bookings { get; set; }

        public DbSet<Payment> Payments { get; set; }

        public DbSet<Review> Reviews { get; set; }

        public DbSet<Notification> Notifications { get; set; }

        public DbSet<RefreshToken> RefreshTokens { get; set; }

        public DbSet<VerificationCode> VerificationCodes { get; set; }

        public DbSet<PasswordResetToken> PasswordResetTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User
            modelBuilder.Entity<ApplicationUser>(user =>
            {
                user.HasMany(u => u.Services)
                    .WithOne(s => s.User)
                    .HasForeignKey(s => s.UserId);

                user.HasQueryFilter(u => !u.IsDeleted);
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

                service.HasMany(s => s.Images)
                       .WithOne(i => i.Service)
                       .HasForeignKey(i => i.ServiceId)
                       .OnDelete(DeleteBehavior.Cascade);
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
                review.HasOne(r => r.Service)
                      .WithMany(s => s.Reviews)
                      .HasForeignKey(r => r.ServiceId)
                      .OnDelete(DeleteBehavior.Restrict); // or DeleteBehavior.NoAction

                review.HasOne(r => r.Booking)
                      .WithMany()
                      .HasForeignKey(r => r.BookingId)
                      .OnDelete(DeleteBehavior.Cascade);

                review.HasOne(r => r.User)
                      .WithMany(u => u.Reviews)
                      .HasForeignKey(r => r.UserId)
                      .OnDelete(DeleteBehavior.Restrict);

                review.HasQueryFilter(r => !r.IsDeleted);
            });

            // NOTIFICATION
            modelBuilder.Entity<Notification>(notification =>
            {
                notification.HasOne(n => n.User)
                            .WithMany(u => u.Notifications) // Make sure User has ICollection<Notification>
                            .HasForeignKey(n => n.UserId)
                            .OnDelete(DeleteBehavior.Cascade);
            });

            // VERIFICATION CODE
            modelBuilder.Entity<VerificationCode>(vc =>
            {
                vc.HasIndex(v => v.Email);
                vc.Property(v => v.Code).IsRequired().HasMaxLength(5);
            });
        }
    }
}
