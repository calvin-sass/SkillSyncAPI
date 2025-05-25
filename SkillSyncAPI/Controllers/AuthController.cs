using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SkillSyncAPI.Domain.DTOs.Users;
using SkillSyncAPI.Domain.Entities;
using SkillSyncAPI.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace SkillSyncAPI.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IConfiguration _configuration;

        public AuthController(IAuthService authService, IConfiguration configuration)
        {
            _authService = authService;
            _configuration = configuration;
        }

        // STEP 1: Request signup code (user details + email)
        // POST: api/v1/Auth/request-signup-code
        [HttpPost("request-signup-code")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RequestSignupCode([FromBody] UserRegisterDto dto)
        {
            if (dto == null)
                return BadRequest(new { message = "Invalid request body." });

            // Check if user already exists
            var existingUser = await _authService.FindUserByEmailAsync(dto.Email);
            if (existingUser != null)
                return BadRequest(new { message = "Email already used." });

            var (success, error) = await _authService.CreatePendingUserAndSendCodeAsync(dto);
            if (!success)
                return BadRequest(new { message = error });

            return Ok(new { message = "Verification code sent to your email." });
        }

        // STEP 2: Confirm code and create account
        // POST: api/v1/Auth/confirm-signup-code
        [HttpPost("confirm-signup-code")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ConfirmSignupCode([FromBody] ConfirmEmailDto dto)
        {
            if (dto == null)
                return BadRequest(new { message = "Invalid request body." });

            var (success, error) = await _authService.ConfirmPendingUserAndCreateAccountAsync(dto.Email, dto.Token);
            if (!success)
                return BadRequest(new { message = error });

            return Ok(new { message = "Account created and email confirmed. You can now log in." });
        }

        // LOGIN
        // POST: api/v1/Auth/login
        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] UserLoginDto dto)
        {
            if (dto == null)
                return BadRequest(new { message = "Invalid request body." });

            var (user, token, refreshToken, error) = await _authService.LoginAsync(dto);
            if (user == null)
                return Unauthorized(new { message = error });

            if (!user.EmailConfirmed)
                return BadRequest(new { message = "Please confirm your email before logging in." });

            return Ok(new
            {
                token,
                refreshToken,
                user = new
                {
                    id = user.Id,
                    username = user.UserName,
                    email = user.Email,
                    role = user.Role
                }
            });
        }

        // REFRESH TOKEN
        // POST: api/v1/Auth/refresh
        [HttpPost("refresh")]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenDto dto)
        {
            if (dto == null || string.IsNullOrEmpty(dto.Token) || string.IsNullOrEmpty(dto.Email))
                return BadRequest(new { message = "Invalid refresh token data." });

            var (token, newRefreshToken, error) = await _authService.RefreshTokenAsync(dto.Email, dto.Token);
            if (token == null)
                return Unauthorized(new { message = error });

            return Ok(new { token, refreshToken = newRefreshToken });
        }

        // FORGOT PASSWORD
        [HttpPost("request-password-reset")]
        public async Task<IActionResult> RequestPasswordReset([FromBody] string email)
        {
            var user = await _authService.FindUserByEmailAsync(email);
            if (user == null)
                return Ok(new { message = "If the email exists, a reset link has been sent." }); // Don't reveal user existence

            // Use frontend URL from configuration instead of API URL
            string frontendUrl = _configuration["FrontendUrl"] ?? "http://localhost:5173";
            var (success, error) = await _authService.SendPasswordResetLinkAsync(email, frontendUrl);
            if (!success)
                return BadRequest(new { message = error });

            return Ok(new { message = "If the email exists, a reset link has been sent." });
        }

        // RESET PASSWORD
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            var (success, error) = await _authService.ResetPasswordWithTokenAsync(dto.Email, dto.Token, dto.NewPassword);
            if (!success)
                return BadRequest(new { message = error });

            return Ok(new { message = "Password reset successfully." });
        }
    }
}
