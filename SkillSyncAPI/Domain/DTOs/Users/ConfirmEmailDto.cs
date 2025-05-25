using System.ComponentModel.DataAnnotations;

namespace SkillSyncAPI.Domain.DTOs.Users
{
    public class ConfirmEmailDto
    {
        [Required, EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Token { get; set; }
    }
}
