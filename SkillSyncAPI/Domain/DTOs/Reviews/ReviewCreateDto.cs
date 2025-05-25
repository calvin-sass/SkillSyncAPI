using System.ComponentModel.DataAnnotations;

namespace SkillSyncAPI.Domain.DTOs.Reviews
{
    public class ReviewCreateDto
    {
        public int ServiceId { get; set; }

        public int BookingId { get; set; } 

        public decimal Rating { get; set; } 

        public string? Comment { get; set; }
    }
}
