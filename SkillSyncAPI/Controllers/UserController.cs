using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SkillSyncAPI.Domain.DTOs.Users;
using SkillSyncAPI.Services;
using System.Security.Claims;

namespace SkillSyncAPI.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;

        public UserController(IUserService userService, IEmailService emailService, IConfiguration configuration)
        {
            _userService = userService;
            _emailService = emailService;
            _configuration = configuration;
        }

        // GET: api/v1/user/me
        [Authorize]
        [HttpGet("me")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<UserProfileDto>> GetCurrentUser()
        {
            var userId = User.FindFirstValue("id");
            var userProfile = await _userService.GetCurrentUserAsync(userId);
            if (userProfile == null)
                return NotFound("User not found.");

            return Ok(userProfile);
        }


        // PATCH: api/v1/user/update
        [Authorize]
        [HttpPatch("update")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateAccount(UpdateUserDto dto)
        {
            var userId = User.FindFirstValue("id");
            var (success, errors, profile) = await _userService.UpdateAccountAsync(userId, dto);
            if (!success)
                return NotFound(errors?.FirstOrDefault() ?? "User not found");
            return Ok(profile);
        }

        // ChangePassword
        [Authorize]
        [HttpPost("change-password")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.CurrentPassword) || string.IsNullOrWhiteSpace(dto.NewPassword))
                return BadRequest("Current and new password are required.");

            var userId = User.FindFirstValue("id");
            var (success, errors) = await _userService.ChangePasswordAsync(userId, dto.CurrentPassword, dto.NewPassword);
            if (!success)
                return BadRequest(errors);

            // Get user email for notification
            var userProfile = await _userService.GetCurrentUserAsync(userId);
            if (userProfile != null)
            {
                await _emailService.SendAsync(
                    userProfile.Email,
                    "Password Changed",
                    "Your password was changed successfully. If you did not perform this action, please contact support immediately."
                );
            }

            return Ok(new { message = "Password changed successfully." });
        }

        // Upload profile photo
        [Authorize]
        [HttpPost("avatar")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UploadAvatar([FromForm] UserImageAvatarUploadDto avatar)
        {
            var userId = User.FindFirstValue("id");
            var (success, avatarUrl, errors) = await _userService.UploadAvatarAsync(userId, avatar.Avatar);
            if (!success)
                return BadRequest(errors);
            return Ok(new { avatarUrl });
        }

        // DELETE: api/v1/user/delete
        [Authorize]
        [HttpDelete("delete")]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> DeleteAccount()
        {
            var userId = User.FindFirstValue("id");
            var (success, errors, reactivationToken, email) = await _userService.DeleteAccountAsync(userId);
            if (!success)
                return BadRequest(errors);

            if (!string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(reactivationToken))
            {
                // Create reactivation link
                string frontendUrl = _configuration["FrontendUrl"] ?? "http://localhost:5173";
                var reactivationLink = $"{frontendUrl}/account-reactivation?email={Uri.EscapeDataString(email)}&token={reactivationToken}";

                var emailBody = $@"
            <p>Your account has been marked as deleted. If you did not perform this action, please contact support immediately.</p>
            <p>If you would like to reactivate your account, you can do so by clicking the link below within the next 7 days:</p>
            <p><a href='{reactivationLink}'>Reactivate My Account</a></p>
            <p>This link will expire in 7 days.</p>
        ";

                await _emailService.SendAsync(
                    email,
                    "Account Deleted - Reactivation Link",
                    emailBody
                );
            }

            return Ok("Account marked as deleted successfully.");
        }

        // POST: api/v1/user/reactivate
        [HttpPost("reactivate")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ReactivateAccount()
        {
            var userId = User.FindFirstValue("id");
            var (success, errors) = await _userService.ReactivateAccountAsync(userId);
            if (!success)
                return BadRequest(errors);

            // Get user email for notification
            var userProfile = await _userService.GetCurrentUserAsync(userId);
            if (userProfile != null)
            {
                await _emailService.SendAsync(
                    userProfile.Email,
                    "Account Reactivated",
                    "Your account has been reactivated. Welcome back!"
                );
            }

            return Ok("Account reactivated successfully.");
        }

        // POST: api/v1/user/reactivate-by-token
        [HttpPost("reactivate-by-token")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ReactivateAccountByToken([FromBody] RefreshTokenDto dto)
        {
            if (string.IsNullOrEmpty(dto.Email) || string.IsNullOrEmpty(dto.Token))
                return BadRequest("Email and token are required.");

            var (success, errors) = await _userService.ReactivateAccountByTokenAsync(dto.Email, dto.Token);
            if (!success)
                return BadRequest(errors);

            // Send confirmation email
            await _emailService.SendAsync(
                dto.Email,
                "Account Reactivated",
                "Your account has been reactivated successfully. You can now log in to your account."
            );

            return Ok("Account reactivated successfully. You can now log in.");
        }

        // GET: /account/reactivate
        [HttpGet]
        [AllowAnonymous]
        [Route("/account/reactivate")]
        public IActionResult RedirectToFrontendReactivation([FromQuery] string email, [FromQuery] string token)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(token))
                return BadRequest("Invalid reactivation link");

            // Get frontend URL from configuration
            string frontendUrl = _configuration["FrontendUrl"] ?? "http://localhost:5173";

            // Construct the frontend URL with the parameters
            var redirectUrl = $"{frontendUrl}/account-reactivation?email={Uri.EscapeDataString(email)}&token={token}";

            // Redirect to the frontend
            return Redirect(redirectUrl);
        }

    }
}
