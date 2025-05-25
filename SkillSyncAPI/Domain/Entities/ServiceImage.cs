using System.ComponentModel.DataAnnotations;

namespace SkillSyncAPI.Domain.Entities
{
    public class ServiceImage
    {
        public int Id { get; set; }
        [Required]
        public string ImageUrl { get; set; } // Path or URL to the image

        // Foreign Key
        public int ServiceId { get; set; }
        public Service Service { get; set; }
    }
}
