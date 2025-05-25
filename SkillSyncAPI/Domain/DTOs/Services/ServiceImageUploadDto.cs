using System.ComponentModel.DataAnnotations;

namespace SkillSyncAPI.Domain.DTOs.Services
{
    public class ServiceImageUploadDto
    {
        [Required]
        public IFormFile Image { get; set; }

        [Required]
        public int ServiceId { get; set; }
    }
}
