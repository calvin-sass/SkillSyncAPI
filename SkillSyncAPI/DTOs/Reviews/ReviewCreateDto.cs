using System.ComponentModel.DataAnnotations;

namespace SkillSyncAPI.DTOs.Reviews
{
    public class ReviewCreateDto
    {
        [Required]
        [Range(1, 5)]
        public int Rating { get; set; }

        public string? Comment { get; set; }

        [Required]
        public int ServiceId { get; set; }
    }
}
