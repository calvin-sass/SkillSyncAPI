using SkillSyncAPI.Domain.DTOs.Users;
using SkillSyncAPI.Domain.Entities;

namespace SkillSyncAPI.Services
{
    public interface IUserService
    {
        Task<UserProfileDto?> GetCurrentUserAsync(string userId);

        Task<(bool Success, IEnumerable<string>? Errors, UserProfileDto? Profile)> UpdateAccountAsync(string userId, UpdateUserDto dto);

        Task<(bool Success, IEnumerable<string>? Errors)> ChangePasswordAsync(string userId, string currentPassword, string newPassword);

        Task<(bool Success, string? AvatarUrl, IEnumerable<string>? Errors)> UploadAvatarAsync(string userId, IFormFile avatar);

        Task<(bool Success, IEnumerable<string>? Errors, string? ReactivationToken, string? Email)> DeleteAccountAsync(string userId);

        Task<(bool Success, IEnumerable<string>? Errors)> ReactivateAccountAsync(string userId);

        Task<(bool Success, string Token, DateTime ExpiresAt)> GenerateReactivationTokenAsync(string userId, string email);

        Task<(bool Success, IEnumerable<string>? Errors)> ReactivateAccountByTokenAsync(string email, string token);
    }
}
