using SkillSyncAPI.Helpers.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.Eventing.Reader;

namespace SkillSyncAPI.Models
{
    public class User : IAuditable
    {
        public int Id { get; set; }

        public string Username { get; set; }

        [Required]
        public string Email { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        [Required]
        public string Role { get; set; } = "User"; // "User" or "Admin"

        public string? Phone { get; set; }

        public string? Address { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public bool IsDeleted { get; set; } = false;

        // Navigation
        public ICollection<Service> Services { get; set; }
        public ICollection<Booking> Bookings { get; set; }
        public ICollection<Review> Reviews { get; set; }
        public ICollection<Notification> Notifications { get; set; }
    }
}
