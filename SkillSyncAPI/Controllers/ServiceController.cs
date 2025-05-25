using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkillSyncAPI.Data;
using SkillSyncAPI.Domain.DTOs.Services;
using SkillSyncAPI.Domain.Entities;
using SkillSyncAPI.Repositories;
using SkillSyncAPI.Services;
using System.Text.RegularExpressions;

namespace SkillSyncAPI.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class ServiceController : ControllerBase
    {
        private readonly IServiceService _serviceService;

        public ServiceController(IServiceService serviceService)
        {
            _serviceService = serviceService;
        }

        // POST: api/v1/service ...Add service = SELLER
        [HttpPost]
        [Authorize(Roles = "Seller")]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateService(ServiceCreateDto dto)
        {
            var userIdClaim = User.FindFirst("id")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int sellerId))
                return Unauthorized("401 Unauthorized.");

            var serviceDto = await _serviceService.CreateServiceAsync(dto, sellerId);
            return CreatedAtAction(nameof(GetService), new { id = serviceDto.Id }, serviceDto);
        }

        // GET: api/v1/service/images/upload ...UPLOAD image
        [HttpPost("images/upload")]
        [Authorize(Roles = "Seller")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UploadServiceImage([FromForm] ServiceImageUploadDto dto)
        {
            var userIdClaim = User.FindFirst("id")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int sellerId))
                return Unauthorized("401 Unauthorized.");

            var (success, imageUrl, error) = await _serviceService.UploadServiceImageAsync(dto.ServiceId, sellerId, dto.Image);
            if (!success)
                return BadRequest(new { message = error });

            return Ok(new { imageUrl });
        }

        [HttpGet("seller")]
        [Authorize(Roles = "Seller")]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<ServiceDto>>> GetSellerServices()
        {
            var userIdClaim = User.FindFirst("id")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int sellerId))
                return Unauthorized("401 Unauthorized.");

            var services = await _serviceService.GetSellerServicesAsync(sellerId);
            return Ok(services);
        }

        // GET: api/v1/service/1  ...get service with a specific ID
        [HttpGet("{id}")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<ServiceDto>> GetService(int id)
        {
            var service = await _serviceService.GetServiceByIdAsync(id);
            if (service == null)
                return NotFound();
            return Ok(service);
        }

        // GET: /api/service?category=Development&priceRange=100-500&search=barber ...GET all services or filter services
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<ServiceDto>>> GetFilteredServices(
            [FromQuery] string? category,
            [FromQuery] string? priceRange,
            [FromQuery] string? search)
        {
            var services = await _serviceService.GetFilteredServicesAsync(category, priceRange, search);
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
            var userIdClaim = User.FindFirst("id")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int sellerId))
                return Unauthorized("401 Unauthorized.");

            var success = await _serviceService.UpdateServiceAsync(id, dto, sellerId);
            if (!success)
                return NotFound();
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
            var userIdClaim = User.FindFirst("id")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int sellerId))
                return Unauthorized("401 Unauthorized.");

            var success = await _serviceService.PatchServiceAsync(id, dto, sellerId);
            if (!success)
                return NotFound();
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
            var userIdClaim = User.FindFirst("id")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int sellerId))
                return Unauthorized("401 Unauthorized.");

            var success = await _serviceService.DeleteServiceAsync(id, sellerId);
            if (!success)
                return NotFound();
            return Ok(new { message = "Service deleted." });
        }

        // DELETE: /{serviceId}/images/{imageId}
        [HttpDelete("{serviceId}/images/{imageId}")]
        [Authorize(Roles = "Seller")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> DeleteServiceImage(int serviceId, int imageId)
        {
            var userIdClaim = User.FindFirst("id")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int sellerId))
                return Unauthorized("401 Unauthorized.");

            // Check if service belongs to seller and delete the image
            var success = await _serviceService.DeleteServiceImageAsync(serviceId, imageId, sellerId);
            if (!success)
                return NotFound();

            return Ok(new { message = "Service image deleted successfully." });
        }

    }
}
