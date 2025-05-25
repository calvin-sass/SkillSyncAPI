using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace SkillSyncAPI.Domain.Entities
{
    public class ApplicationUser : IdentityUser<int> 
    {
        public string? AvatarUrl { get; set; }

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
