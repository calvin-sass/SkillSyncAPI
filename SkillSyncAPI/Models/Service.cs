using SkillSyncAPI.Helpers.Interfaces;

namespace SkillSyncAPI.Models
{
    public class Service : IAuditable
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public string? Description { get; set; }

        public decimal Price { get; set; }

        public string Category { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        // Foreign Key
        public int UserId { get; set; }

        public User User { get; set; }

        public ICollection<Booking> Bookings { get; set; }

        public ICollection<Review> Reviews { get; set; }
    }
}
