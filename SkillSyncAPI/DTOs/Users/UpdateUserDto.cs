using System.ComponentModel.DataAnnotations;

namespace SkillSyncAPI.DTOs.Users
{
    public class UpdateUserDto
    {
        public string? Username { get; set; }

        [EmailAddress]
        public string? Email { get; set; }

        public string? Phone { get; set; }

        public string? Address { get; set; }
    }
}
