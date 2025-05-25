using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillSyncAPI.Domain.DTOs.Bookings;
using SkillSyncAPI.Domain.DTOs.Payments;
using SkillSyncAPI.Services;

namespace SkillSyncAPI.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class BookingController : ControllerBase
    {
        private readonly IBookingService _bookingService;

        public BookingController(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateBooking([FromBody] BookingCreateDto dto)
        {
            var userIdClaim = User.FindFirst("id")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                return Unauthorized();

            var booking = await _bookingService.CreateBookingAsync(userId, dto);
            if (booking == null)
                return BadRequest("Service not found.");

            var bookingDto = new BookingDto
            {
                Id = booking.Id,
                BookingDate = booking.BookingDate,
                Status = booking.Status,
                ServiceId = booking.ServiceId,
                UserId = booking.UserId
            };

            return Ok(bookingDto);
        }

        [Authorize(Roles = "Seller")]
        [HttpPut("{id}/date")]
        public async Task<IActionResult> UpdateBookingDate(int id, [FromBody] DateTime newDate)
        {
            var sellerIdClaim = User.FindFirst("id")?.Value;
            if (string.IsNullOrEmpty(sellerIdClaim) || !int.TryParse(sellerIdClaim, out int sellerId))
                return Unauthorized();

            var success = await _bookingService.UpdateBookingDateAsync(id, sellerId, newDate);
            if (!success)
                return BadRequest("Cannot update booking date. Check payment status or permissions.");

            return Ok(new { message = "Booking date updated." });
        }

        // USER: Get all their bookings
        [Authorize]
        [HttpGet("my")]
        public async Task<IActionResult> GetMyBookings()
        {
            var userIdClaim = User.FindFirst("id")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                return Unauthorized();

            var bookings = await _bookingService.GetBookingsForUserAsync(userId);
            return Ok(bookings);
        }

        // SELLER: Get all bookings for their services
        [Authorize(Roles = "Seller")]
        [HttpGet("seller")]
        public async Task<IActionResult> GetSellerBookings()
        {
            var sellerIdClaim = User.FindFirst("id")?.Value;
            if (string.IsNullOrEmpty(sellerIdClaim) || !int.TryParse(sellerIdClaim, out int sellerId))
                return Unauthorized();

            var bookings = await _bookingService.GetBookingsForSellerAsync(sellerId);
            return Ok(bookings);
        }

        // BOTH: Cancel a booking
        [Authorize]
        [HttpPatch("{id}/cancel")]
        public async Task<IActionResult> CancelBooking(int id)
        {
            var userIdClaim = User.FindFirst("id")?.Value;
            var role = User.FindFirst("http://schemas.microsoft.com/ws/2008/06/identity/claims/role")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                return Unauthorized();

            bool isSeller = role == "Seller";
            var success = await _bookingService.CancelBookingAsync(id, userId, isSeller);
            if (!success)
                return BadRequest("You are not authorized to cancel this booking or booking not found.");

            return Ok(new { message = "Booking cancelled." });
        }
    }
}
