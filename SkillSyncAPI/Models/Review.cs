using SkillSyncAPI.Helpers.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace SkillSyncAPI.Models
{
    public class Review : IAuditable
    {
        public int Id { get; set; }

        [Range(1, 5)]
        public int Rating { get; set; }

        public string? Comment { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        // Foreign Keys
        public int UserId { get; set; }

        public User User { get; set; }

        public int ServiceId { get; set; }

        public Service Service { get; set; }
    }
}
