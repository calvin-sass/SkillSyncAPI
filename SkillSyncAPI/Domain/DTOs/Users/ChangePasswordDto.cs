namespace SkillSyncAPI.Domain.DTOs.Users
{
    public class ChangePasswordDto
    {
        public string CurrentPassword { get; set; }

        public string NewPassword { get; set; }
    }
}
