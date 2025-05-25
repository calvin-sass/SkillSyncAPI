using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SkillSyncAPI.Data;
using SkillSyncAPI.Domain.DTOs.Users;
using SkillSyncAPI.Domain.Entities;

namespace SkillSyncAPI.Services.Impl
{
    public class UserService : IUserService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly Cloudinary _cloudinary;

        public UserService(UserManager<ApplicationUser> userManager, ApplicationDbContext context, Cloudinary cloudinary)
        {
            _userManager = userManager;
            _context = context;
            _cloudinary = cloudinary;
        }

        public async Task<UserProfileDto?> GetCurrentUserAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return null;
            return new UserProfileDto
            {
                Username = user.UserName,
                Email = user.Email,
                Phone = user.Phone,
                Address = user.Address,
                AvatarUrl = user.AvatarUrl,
                CreatedAt = user.CreatedAt
            };
        }

        public async Task<(bool Success, IEnumerable<string>? Errors, UserProfileDto? Profile)> UpdateAccountAsync(string userId, UpdateUserDto dto)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return (false, new[] { "User not found" }, null);

            if (!string.IsNullOrWhiteSpace(dto.Username)) user.UserName = dto.Username;
            if (!string.IsNullOrWhiteSpace(dto.Email)) user.Email = dto.Email;
            if (!string.IsNullOrWhiteSpace(dto.Phone)) user.Phone = dto.Phone;
            if (!string.IsNullOrWhiteSpace(dto.Address)) user.Address = dto.Address;

            user.UpdatedAt = DateTime.UtcNow;
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                return (false, result.Errors.Select(e => e.Description), null);

            var profile = new UserProfileDto
            {
                Username = user.UserName,
                Email = user.Email,
                Phone = user.Phone,
                Address = user.Address,
                CreatedAt = user.CreatedAt
            };
            return (true, null, profile);
        }

        public async Task<(bool Success, IEnumerable<string>? Errors)> ChangePasswordAsync(string userId, string currentPassword, string newPassword)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return (false, new[] { "User not found" });

            var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
            if (!result.Succeeded)
                return (false, result.Errors.Select(e => e.Description));
            return (true, null);
        }

        public async Task<(bool Success, string? AvatarUrl, IEnumerable<string>? Errors)> UploadAvatarAsync(string userId, IFormFile avatar)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return (false, null, new[] { "User not found" });

            if (avatar == null || avatar.Length == 0)
                return (false, null, new[] { "No file uploaded." });

            // Upload to Cloudinary
            using var stream = avatar.OpenReadStream();
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(avatar.FileName, stream),
                Folder = "avatars"
            };
            var uploadResult = await _cloudinary.UploadAsync(uploadParams);

            if (uploadResult.StatusCode != System.Net.HttpStatusCode.OK)
                return (false, null, new[] { "Cloudinary upload failed." });

            user.AvatarUrl = uploadResult.SecureUrl.ToString();
            user.UpdatedAt = DateTime.UtcNow;
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                return (false, null, result.Errors.Select(e => e.Description));

            return (true, user.AvatarUrl, null);
        }

        public async Task<(bool Success, IEnumerable<string>? Errors, string? ReactivationToken, string? Email)> DeleteAccountAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return (false, new[] { "User not found." }, null, null);

            if (user.IsDeleted)
                return (false, new[] { "Account already deleted." }, null, null);

            user.IsDeleted = true;
            user.UpdatedAt = DateTime.UtcNow;
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                return (false, result.Errors.Select(e => e.Description), null, null);

            // Generate reactivation token
            var (tokenSuccess, token, _) = await GenerateReactivationTokenAsync(userId, user.Email);
            if (!tokenSuccess)
                return (true, null, null, null); // Still return success for account deletion

            return (true, null, token, user.Email);
        }

        public async Task<(bool Success, IEnumerable<string>? Errors)> ReactivateAccountAsync(string userId)
        {
            var user = await _userManager.Users.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.Id.ToString() == userId);
            if (user == null) return (false, new[] { "User not found." });

            if (!user.IsDeleted)
                return (false, new[] { "Account is already active." });

            user.IsDeleted = false;
            user.UpdatedAt = DateTime.UtcNow;
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                return (false, result.Errors.Select(e => e.Description));

            return (true, null);
        }

        public async Task<(bool Success, string Token, DateTime ExpiresAt)> GenerateReactivationTokenAsync(string userId, string email)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || !user.IsDeleted)
                return (false, string.Empty, DateTime.MinValue);

            // Generate a token and save it using the existing PasswordResetToken table
            var token = Guid.NewGuid().ToString("N");
            var expiresAt = DateTime.UtcNow.AddDays(7); // 7-day expiration

            _context.PasswordResetTokens.Add(new PasswordResetToken
            {
                Email = email,
                Token = token,
                ExpiresAt = expiresAt
            });
            await _context.SaveChangesAsync();

            return (true, token, expiresAt);
        }

        public async Task<(bool Success, IEnumerable<string>? Errors)> ReactivateAccountByTokenAsync(string email, string token)
        {
            // Find the token
            var resetToken = await _context.PasswordResetTokens
                .FirstOrDefaultAsync(t => t.Email == email && t.Token == token && t.ExpiresAt > DateTime.UtcNow);

            if (resetToken == null)
                return (false, new[] { "Invalid or expired reactivation token." });

            // Find the user by email (ignoring the IsDeleted filter)
            var user = await _userManager.Users.IgnoreQueryFilters()
                .FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
                return (false, new[] { "User not found." });

            if (!user.IsDeleted)
                return (false, new[] { "Account is already active." });

            // Reactivate the account
            user.IsDeleted = false;
            user.UpdatedAt = DateTime.UtcNow;
            var result = await _userManager.UpdateAsync(user);

            // Remove the used token
            _context.PasswordResetTokens.Remove(resetToken);
            await _context.SaveChangesAsync();

            if (!result.Succeeded)
                return (false, result.Errors.Select(e => e.Description));

            return (true, null);
        }
    }
}
