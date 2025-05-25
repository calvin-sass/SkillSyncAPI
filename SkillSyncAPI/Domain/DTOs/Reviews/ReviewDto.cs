namespace SkillSyncAPI.Domain.DTOs.Reviews
{
    public class ReviewDto
    {
        public int Id { get; set; }

        public int ServiceId { get; set; }

        public int BookingId { get; set; } 

        public int UserId { get; set; }

        public decimal Rating { get; set; } 

        public string? Comment { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}
