using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillSyncAPI.Domain.DTOs.Payments;
using SkillSyncAPI.Services;

namespace SkillSyncAPI.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;

        public PaymentController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        // Make Payment
        [Authorize]
        [HttpPost("booking/{bookingId}")]
        public async Task<IActionResult> PayForBooking(int bookingId, [FromBody] PaymentCreateDto dto)
        {
            var userIdClaim = User.FindFirst("id")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                return Unauthorized();

            dto.BookingId = bookingId;
            var (success, error) = await _paymentService.ProcessStripePaymentAsync(userId, dto);

            if (!success)
            {
                // Return more specific error information
                return BadRequest(new
                {
                    message = error,
                    success = false
                });
            }

            // Return a response that matches the frontend PaymentResponseDto
            return Ok(new
            {
                message = "Payment successful.",
                success = true
            });
        }
    }
}
