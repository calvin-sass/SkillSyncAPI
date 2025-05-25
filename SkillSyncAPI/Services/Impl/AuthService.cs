using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SkillSyncAPI.Data;
using SkillSyncAPI.Domain.DTOs.Users;
using SkillSyncAPI.Domain.Entities;
using SkillSyncAPI.Repositories;
using SkillSyncAPI.Services.Impl;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SkillSyncAPI.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IUserRepository _userRepository;
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IUserRepository userRepository,
            IConfiguration configuration,
            ApplicationDbContext context,
            IEmailService emailService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _userRepository = userRepository;
            _configuration = configuration;
            _context = context;
            _emailService = emailService;
        }

        // Step 1: Store pending user and send code
        public async Task<(bool Success, string? Error)> CreatePendingUserAndSendCodeAsync(UserRegisterDto dto)
        {
            // Remove any existing pending user for this email
            var existing = await _context.PendingUsers.FirstOrDefaultAsync(p => p.Email == dto.Email);
            if (existing != null)
            {
                _context.PendingUsers.Remove(existing);
                await _context.SaveChangesAsync();
            }

            var code = Generate5DigitCode();
            var pending = new PendingUser
            {
                Email = dto.Email,
                Username = dto.Username,
                Password = dto.Password,
                Phone = dto.Phone,
                Address = dto.Address,
                Role = dto.Role,
                Code = code,
                ExpiresAt = DateTime.UtcNow.AddMinutes(10)
            };
            _context.PendingUsers.Add(pending);
            await _context.SaveChangesAsync();

            await _emailService.SendAsync(dto.Email, "Your Verification Code", $"Your code is: {code}");
            return (true, null);
        }

        // Step 2: Confirm code and create user
        public async Task<(bool Success, string? Error)> ConfirmPendingUserAndCreateAccountAsync(string email, string code)
        {
            var pending = await _context.PendingUsers.FirstOrDefaultAsync(p => p.Email == email && p.Code == code && p.ExpiresAt > DateTime.UtcNow);
            if (pending == null)
                return (false, "Invalid or expired code.");

            var existingUser = await _userManager.FindByEmailAsync(email);
            if (existingUser != null)
                return (false, "Account already exists.");

            var user = new ApplicationUser
            {
                UserName = pending.Username,
                Email = pending.Email,
                Phone = pending.Phone,
                Address = pending.Address,
                Role = pending.Role,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user);
            if (!result.Succeeded)
                return (false, string.Join("; ", result.Errors.Select(e => e.Description)));

            await _userManager.AddPasswordAsync(user, pending.Password);
            await _userManager.AddToRoleAsync(user, pending.Role);

            _context.PendingUsers.Remove(pending);
            await _context.SaveChangesAsync();

            return (true, null);
        }

        // Login
        public async Task<(ApplicationUser user, string token, string refreshToken, string error)> LoginAsync(UserLoginDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
                return (null, null, null, "User not found.");

            var result = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, false);
            if (!result.Succeeded)
                return (null, null, null, "Invalid credentials.");

            var token = GenerateJwtToken(user);
            var refreshToken = GenerateRefreshToken();

            // Store the refresh token
            var refreshTokenEntity = new RefreshToken
            {
                Token = refreshToken,
                Expires = DateTime.UtcNow.AddDays(7),
                UserId = user.Id,
                IsRevoked = false
            };
            _context.RefreshTokens.Add(refreshTokenEntity);
            await _context.SaveChangesAsync();

            return (user, token, refreshToken, null);
        }

        // JWT Token
        public string GenerateJwtToken(ApplicationUser user)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                new Claim("id", user.Id.ToString()),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.UtcNow.AddMinutes(30);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: expires,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        // Refresh Token
        private string GenerateRefreshToken()
        {
            var randomBytes = new byte[64];
            System.Security.Cryptography.RandomNumberGenerator.Fill(randomBytes);
            return Convert.ToBase64String(randomBytes);
        }

        public async Task<(ApplicationUser? User, string? Token, string? RefreshToken, string? Error)> LoginWithRefreshAsync(UserLoginDto dto)
        {
            var user = _userRepository.GetAll().FirstOrDefault(u => u.Email == dto.Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                return (null, null, null, "Invalid credentials.");

            var token = GenerateJwtToken(user);
            var refreshToken = GenerateRefreshToken();

            var refreshTokenEntity = new RefreshToken
            {
                Token = refreshToken,
                Expires = DateTime.UtcNow.AddMinutes(15),
                UserId = user.Id,
                IsRevoked = false
            };
            _context.RefreshTokens.Add(refreshTokenEntity);
            await _context.SaveChangesAsync();

            return (user, token, refreshToken, null);
        }

        public async Task<(string? Token, string? RefreshToken, string? Error)> RefreshTokenAsync(string email, string refreshToken)
        {
            // First find the user by email
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return (null, null, "User not found.");

            // Then find the refresh token for this user
            var storedToken = await _context.RefreshTokens
                .FirstOrDefaultAsync(x => x.Token == refreshToken && x.UserId == user.Id && !x.IsRevoked);

            if (storedToken == null)
                return (null, null, "Invalid refresh token.");

            if (storedToken.Expires < DateTime.UtcNow)
                return (null, null, "Refresh token has expired.");

            // Revoke the current token
            storedToken.IsRevoked = true;

            // Generate new tokens
            var newJwt = GenerateJwtToken(user);
            var newRefresh = GenerateRefreshToken();

            // Store the new refresh token
            var newRefreshEntity = new RefreshToken
            {
                Token = newRefresh,
                Expires = DateTime.UtcNow.AddDays(7),
                UserId = user.Id,
                IsRevoked = false
            };
            _context.RefreshTokens.Add(newRefreshEntity);
            await _context.SaveChangesAsync();

            return (newJwt, newRefresh, null);
        }

        public async Task<ApplicationUser?> FindUserByEmailAsync(string email)
        {
            return await _userManager.FindByEmailAsync(email);
        }

        public async Task<(bool Success, IEnumerable<string> Errors)> ResetPasswordAsync(ApplicationUser user, string newPassword)
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
            return (result.Succeeded, result.Errors.Select(e => e.Description));
        }

        private string Generate5DigitCode()
        {
            var random = new Random();
            return random.Next(10000, 99999).ToString();
        }

        // Legacy: Send code for already created user (not used in new flow)
        public async Task<(bool Success, string? Error)> SendEmailVerificationCodeAsync(string email)
        {
            var existing = await _context.VerificationCodes.Where(v => v.Email == email).ToListAsync();
            if (existing.Any())
            {
                _context.VerificationCodes.RemoveRange(existing);
                await _context.SaveChangesAsync();
            }

            var code = Generate5DigitCode();
            var verification = new VerificationCode
            {
                Email = email,
                Code = code,
                ExpiresAt = DateTime.UtcNow.AddMinutes(10)
            };
            _context.VerificationCodes.Add(verification);
            await _context.SaveChangesAsync();

            await _emailService.SendAsync(email, "Your Verification Code", $"Your code is: {code}");

            return (true, null);
        }

        // Legacy: Confirm code for already created user (not used in new flow)
        public async Task<(bool Success, string? Error)> ConfirmEmailWithCodeAsync(string email, string code)
        {
            var verification = await _context.VerificationCodes
                .FirstOrDefaultAsync(v => v.Email == email && v.Code == code && v.ExpiresAt > DateTime.UtcNow);

            if (verification == null)
                return (false, "Invalid or expired code.");

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return (false, "User not found.");

            user.EmailConfirmed = true;
            await _userManager.UpdateAsync(user);

            _context.VerificationCodes.Remove(verification);
            await _context.SaveChangesAsync();

            return (true, null);
        }

        public async Task<(bool Success, string? Error)> SendPasswordResetLinkAsync(string email, string baseUrl)
        {
            // Remove existing tokens
            var existing = await _context.PasswordResetTokens.Where(t => t.Email == email).ToListAsync();
            if (existing.Any())
            {
                _context.PasswordResetTokens.RemoveRange(existing);
                await _context.SaveChangesAsync();
            }

            var token = Guid.NewGuid().ToString("N");
            var expiresAt = DateTime.UtcNow.AddMinutes(5);

            _context.PasswordResetTokens.Add(new PasswordResetToken
            {
                Email = email,
                Token = token,
                ExpiresAt = expiresAt
            });
            await _context.SaveChangesAsync();

            var resetUrl = $"{baseUrl}/reset-password?email={Uri.EscapeDataString(email)}&token={token}";
            await _emailService.SendAsync(email, "Password Reset", $"Click <a href=\"{resetUrl}\">here</a> to reset your password. This link is valid for 5 minutes.");

            return (true, null);
        }

        public async Task<(bool Success, string? Error)> ResetPasswordWithTokenAsync(string email, string token, string newPassword)
        {
            var resetToken = await _context.PasswordResetTokens
                .FirstOrDefaultAsync(t => t.Email == email && t.Token == token && t.ExpiresAt > DateTime.UtcNow);

            if (resetToken == null)
                return (false, "Invalid or expired reset link.");

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return (false, "User not found.");

            var identityToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, identityToken, newPassword);

            if (!result.Succeeded)
                return (false, string.Join("; ", result.Errors.Select(e => e.Description)));

            _context.PasswordResetTokens.Remove(resetToken);
            await _context.SaveChangesAsync();

            return (true, null);
        }
    }
}