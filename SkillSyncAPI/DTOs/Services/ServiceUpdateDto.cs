using System.ComponentModel.DataAnnotations;

namespace SkillSyncAPI.DTOs.Services
{
    public class ServiceUpdateDto
    {
        [Required]
        public string Title { get; set; }

        public string? Description { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }

        public string? Category { get; set; }
    }
}
