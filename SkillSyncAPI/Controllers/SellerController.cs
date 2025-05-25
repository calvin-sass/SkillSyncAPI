using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkillSyncAPI.Data;
using System.Security.Claims;

namespace SkillSyncAPI.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    [Authorize(Roles = "Seller")]
    public class SellerController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public SellerController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetSellerStats()

        {
            // Get current user ID
            var userId = User.FindFirstValue("id");
            if (userId == null || !int.TryParse(userId, out int sellerId))
                return Unauthorized();

            // Check if user is a seller
            var user = await _context.Users.FindAsync(sellerId);
            if (user == null || user.Role.ToLower() != "seller")
                return Forbid();

            try
            {
                // 1. Total services count 
                var totalServices = await _context.Services
                    .Where(s => s.UserId == sellerId)
                    .CountAsync();

                // 2. Total bookings count
                var totalBookings = await _context.Bookings
                    .Include(b => b.Service)
                    .Where(b => b.Service.UserId == sellerId)
                    .CountAsync();

                // 3. Completed bookings count  
                var completedBookings = await _context.Bookings
                    .Include(b => b.Service)
                    .Where(b => b.Service.UserId == sellerId && b.Status == "Completed")
                    .CountAsync();

                // 4. Total sales (earnings) from payments
                var totalSales = await _context.Payments
                    .Include(p => p.Booking)
                    .ThenInclude(b => b.Service)
                    .Where(p => p.Booking.Service.UserId == sellerId && p.Status == "Paid")
                    .SumAsync(p => p.Amount);

                return Ok(new
                {
                    totalServices,
                    totalBookings,
                    completedBookings,
                    totalEarnings = totalSales
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while retrieving seller statistics: {ex.Message}");
            }
        }
    }
}
