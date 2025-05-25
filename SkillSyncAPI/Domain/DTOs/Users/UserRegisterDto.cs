using System.ComponentModel.DataAnnotations;

namespace SkillSyncAPI.Domain.DTOs.Users
{
    public class UserRegisterDto
    {
        [Required]
        public string Username { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [Required, MinLength(6)]
        public string Password { get; set; }

        public string? Phone { get; set; }
        public string? Address { get; set; }

        [Required]
        [RegularExpression("User|Seller", ErrorMessage = "Role must be either 'User' or 'Seller'.")]
        public string Role { get; set; } = "User"; // Default to "User"
    }
}
