using SkillSyncAPI.Domain.DTOs.Users;
using SkillSyncAPI.Domain.Entities;

namespace SkillSyncAPI.Services
{
    public interface IAuthService
    {

        Task<(bool Success, string? Error)> CreatePendingUserAndSendCodeAsync(UserRegisterDto dto);

        Task<(bool Success, string? Error)> ConfirmPendingUserAndCreateAccountAsync(string email, string code);

        Task<(ApplicationUser user, string token, string refreshToken, string error)> LoginAsync(UserLoginDto dto);

        string GenerateJwtToken(ApplicationUser user);

        Task<(ApplicationUser? User, string? Token, string? RefreshToken, string? Error)> LoginWithRefreshAsync(UserLoginDto dto);

        Task<ApplicationUser?> FindUserByEmailAsync(string email);

        Task<(bool Success, IEnumerable<string> Errors)> ResetPasswordAsync(ApplicationUser user, string newPassword);

        Task<(bool Success, string? Error)> SendEmailVerificationCodeAsync(string email);

        Task<(bool Success, string? Error)> ConfirmEmailWithCodeAsync(string email, string code);

        Task<(bool Success, string? Error)> SendPasswordResetLinkAsync(string email, string baseUrl);

        Task<(bool Success, string? Error)> ResetPasswordWithTokenAsync(string email, string token, string newPassword);

        Task<(string? Token, string? RefreshToken, string? Error)> RefreshTokenAsync(string email, string refreshToken);
    }
}
