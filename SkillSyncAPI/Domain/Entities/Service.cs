namespace SkillSyncAPI.Domain.Entities
{
    public class Service
    {
        public int Id { get; set; }

        public ICollection<ServiceImage> Images { get; set; } = new List<ServiceImage>();

        public string Title { get; set; }

        public string? Description { get; set; }

        public decimal Price { get; set; }

        public string Category { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        // Foreign Key
        public int UserId { get; set; }

        public ApplicationUser User { get; set; }

        public ICollection<Booking> Bookings { get; set; }

        public ICollection<Review> Reviews { get; set; }
    }
}
