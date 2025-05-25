using System.ComponentModel.DataAnnotations;

namespace SkillSyncAPI.Domain.Entities
{
    public class Review
    {
        public int Id { get; set; }

        [Range(1, 5)]
        public decimal Rating { get; set; }

        public string? Comment { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        // Foreign Keys
        public int UserId { get; set; }

        public ApplicationUser User { get; set; }

        public int ServiceId { get; set; }

        public Service Service { get; set; }

        public int BookingId { get; set; }

        public Booking Booking { get; set; }

        public bool IsDeleted { get; set; }
    }
}
