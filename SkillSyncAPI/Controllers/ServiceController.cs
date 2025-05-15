using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkillSyncAPI.Data;
using SkillSyncAPI.DTOs.Services;
using SkillSyncAPI.Models;

namespace SkillSyncAPI.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class ServiceController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ServiceController(ApplicationDbContext context)
        {
            _context = context;
        }

        // POST: api/v1/service
        [HttpPost]
        [Authorize(Roles = "Seller")]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> CreateService(ServiceCreateDto dto)
        {
            var userIdClaim = User.FindFirst("id")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int sellerId))
                return Unauthorized("401 Unauthorized.");

            var service = new Service
            {
                Title = dto.Title,
                Description = dto.Description,
                Price = dto.Price,
                Category = dto.Category,
                UserId = sellerId
            };

            _context.Services.Add(service);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Service created", service.Id });
        }

        // GET: api/v1/service/1  ...get service with a specific ID
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<ServiceDto>> GetService(int id)
        {
            var service = await _context.Services.FindAsync(id);
            if (service == null)
                return NotFound();

            return new ServiceDto
            {
                Id = service.Id,
                Title = service.Title,
                Description = service.Description,
                Price = service.Price,
                Category = service.Category,
                CreatedAt = service.CreatedAt,
                UserId = service.UserId
            };
        }

        // GET: /api/service?category=Development&priceRange=100-500 ...GET all services or filter services
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<ServiceDto>>> GetFilteredServices(
            [FromQuery] string? category,
            [FromQuery] string? priceRange,
            [FromQuery] string? search)
        {
            var query = _context.Services.AsQueryable();

            // Filter by category (exact match, case-insensitive)
            if (!string.IsNullOrWhiteSpace(category))
            {
                query = query.Where(s => s.Category.ToLower() == category.ToLower());
            }

            // Filter by price range
            if (!string.IsNullOrWhiteSpace(priceRange))
            {
                if (priceRange.Contains('-'))
                {
                    var parts = priceRange.Split('-');
                    if (decimal.TryParse(parts[0], out var min) && decimal.TryParse(parts[1], out var max))
                    {
                        query = query.Where(s => s.Price >= min && s.Price <= max);
                    }
                }
                else if (priceRange.EndsWith("+"))
                {
                    var minStr = priceRange.TrimEnd('+');
                    if (decimal.TryParse(minStr, out var min))
                    {
                        query = query.Where(s => s.Price >= min);
                    }
                }
            }

            // Filter by search keyword in Title or Description (case-insensitive)
            if (!string.IsNullOrWhiteSpace(search))
            {
                var lowerSearch = search.ToLower();
                query = query.Where(s =>
                    s.Title.ToLower().Contains(lowerSearch));
            }

            var services = await query.Select(s => new ServiceDto
            {
                Id = s.Id,
                Title = s.Title,
                Description = s.Description,
                Price = s.Price,
                Category = s.Category,
                CreatedAt = s.CreatedAt,
                UserId = s.UserId
            }).ToListAsync();

            return Ok(services);
        }


        // POST: api/v1/service/1 ...UPDATE services
        [HttpPut("{id}")]
        [Authorize(Roles = "Seller")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateService(int id, ServiceUpdateDto dto)
        {
            var service = await _context.Services.FindAsync(id);
            if (service == null)
                return NotFound();

            var userIdClaim = User.FindFirst("id")?.Value;
            if (service.UserId.ToString() != userIdClaim)
                return Forbid();

            service.Title = dto.Title;
            service.Description = dto.Description;
            service.Price = dto.Price;
            service.Category = dto.Category;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Service updated." });
        }

        // PATCH: api/v1/service/1 ...PARTIALLY UPDATE specific service
        [HttpPatch("{id}")]
        [Authorize(Roles = "Seller")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> PatchService(int id, ServicePatchDto dto)
        {
            var service = await _context.Services.FindAsync(id);
            if (service == null)
                return NotFound();

            var userIdClaim = User.FindFirst("id")?.Value;
            if (service.UserId.ToString() != userIdClaim)
                return Forbid();

            if (!string.IsNullOrEmpty(dto.Title)) service.Title = dto.Title;
            if (dto.Description != null) service.Description = dto.Description;
            if (dto.Price.HasValue) service.Price = dto.Price.Value;
            if (dto.Category != null) service.Category = dto.Category;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Service patched." });
        }

        // DELETE: api/v1/service/1 ...DELETE specific service
        [HttpDelete("{id}")]
        [Authorize(Roles = "Seller")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> DeleteService(int id)
        {
            var service = await _context.Services.FindAsync(id);
            if (service == null)
                return NotFound();

            var userIdClaim = User.FindFirst("id")?.Value;
            if (service.UserId.ToString() != userIdClaim)
                return Forbid();

            _context.Services.Remove(service);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Service deleted." });
        }

    }
}
