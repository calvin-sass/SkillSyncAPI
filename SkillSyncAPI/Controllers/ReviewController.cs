using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillSyncAPI.Domain.DTOs.Reviews;
using SkillSyncAPI.Services;

namespace SkillSyncAPI.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class ReviewController : ControllerBase
    {
        private readonly IReviewService _reviewService;

        public ReviewController(IReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateReview([FromBody] ReviewCreateDto dto)
        {
            var userIdClaim = User.FindFirst("id")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                return Unauthorized();

            var review = await _reviewService.CreateReviewAsync(userId, dto);
            if (review == null)
                return BadRequest("You can only review a service you have paid for and not already reviewed.");

            return Ok(review);
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateReview(int id, [FromBody] ReviewUpdateDto dto)
        {
            var userIdClaim = User.FindFirst("id")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                return Unauthorized();

            var success = await _reviewService.UpdateReviewAsync(userId, id, dto);
            if (!success)
                return NotFound("Review not found or not owned by user.");

            return Ok(new { message = "Review updated." });
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReview(int id)
        {
            var userIdClaim = User.FindFirst("id")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                return Unauthorized();

            var success = await _reviewService.DeleteReviewAsync(userId, id);
            if (!success)
                return NotFound("Review not found or not owned by user.");

            return Ok(new { message = "Review deleted." });
        }

        [Authorize]
        [HttpGet("service/{serviceId}")]
        public async Task<IActionResult> GetReviewsForService(int serviceId)
        {
            var reviews = await _reviewService.GetReviewsForServiceAsync(serviceId);
            return Ok(reviews);
        }

        [Authorize]
        [HttpGet("user")]
        public async Task<IActionResult> GetReviewsByUser()
        {
            var userIdClaim = User.FindFirst("id")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                return Unauthorized();

            var reviews = await _reviewService.GetReviewsByUserAsync(userId);
            return Ok(reviews);
        }
    }
}
