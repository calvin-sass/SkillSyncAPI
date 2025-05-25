using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.EntityFrameworkCore;
using SkillSyncAPI.Data;
using SkillSyncAPI.Domain.DTOs.Services;
using SkillSyncAPI.Domain.Entities;
using SkillSyncAPI.Repositories;
using System.Text.RegularExpressions;

namespace SkillSyncAPI.Services.Impl
{
    public class ServiceService : IServiceService
    {
        private readonly IServiceRepository _serviceRepository;
        private readonly ApplicationDbContext _context;
        private readonly Cloudinary _cloudinary;

        public ServiceService(IServiceRepository serviceRepository, ApplicationDbContext context, Cloudinary cloudinary)
        {
            _serviceRepository = serviceRepository;
            _context = context;
            _cloudinary = cloudinary;
        }

        public async Task<ServiceDto?> GetServiceByIdAsync(int id)
        {
            var service = await _serviceRepository.Query()
                .Include(s => s.User)
                .Include(s => s.Images)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (service == null) return null;

            return MapToDto(service);
        }

        public async Task<IEnumerable<ServiceDto>> GetFilteredServicesAsync(string? category, string? priceRange, string? search)
        {
            var query = _serviceRepository.Query()
                .Include(s => s.User)
                .Include(s => s.Images)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(category))
                query = query.Where(s => s.Category.ToLower() == category.ToLower());

            if (!string.IsNullOrWhiteSpace(priceRange))
            {
                if (priceRange.Contains('-'))
                {
                    var parts = priceRange.Split('-');
                    if (decimal.TryParse(parts[0], out var min) && decimal.TryParse(parts[1], out var max))
                        query = query.Where(s => s.Price >= min && s.Price <= max);
                }
                else if (priceRange.EndsWith("+"))
                {
                    var minStr = priceRange.TrimEnd('+');
                    if (decimal.TryParse(minStr, out var min))
                        query = query.Where(s => s.Price >= min);
                }
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var cleanSearch = Regex.Replace(search.Trim().ToLower(), @"\s+", " ");
                query = query.Where(s => s.Title.ToLower().Contains(cleanSearch));
            }

            var services = await query.ToListAsync();
            return services.Select(MapToDto);
        }

        public async Task<ServiceDto> CreateServiceAsync(ServiceCreateDto dto, int sellerId)
        {
            var service = new Service
            {
                Title = dto.Title,
                Description = dto.Description,
                Price = dto.Price,
                Category = dto.Category,
                UserId = sellerId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _serviceRepository.Add(service);
            await _serviceRepository.SaveChangesAsync();

            // Fetch with includes for DTO mapping
            var created = await _serviceRepository.Query()
                .Include(s => s.User)
                .Include(s => s.Images)
                .FirstOrDefaultAsync(s => s.Id == service.Id);

            return MapToDto(created!);
        }

        public async Task<(bool Success, string? ImageUrl, string? Error)> UploadServiceImageAsync(int serviceId, int userId, IFormFile image)
        {
            var service = await _context.Services.FindAsync(serviceId);
            if (service == null || service.UserId != userId)
                return (false, null, "Service not found or not owned by user.");

            if (image == null || image.Length == 0)
                return (false, null, "No file uploaded.");

            using var stream = image.OpenReadStream();
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(image.FileName, stream),
                Folder = "service-images"
            };
            var uploadResult = await _cloudinary.UploadAsync(uploadParams);

            if (uploadResult.StatusCode != System.Net.HttpStatusCode.OK)
                return (false, null, "Cloudinary upload failed.");

            var serviceImage = new ServiceImage
            {
                ServiceId = serviceId,
                ImageUrl = uploadResult.SecureUrl.ToString()
            };
            _context.ServiceImages.Add(serviceImage);
            await _context.SaveChangesAsync();

            return (true, serviceImage.ImageUrl, null);
        }

        public async Task<bool> UpdateServiceAsync(int id, ServiceUpdateDto dto, int sellerId)
        {
            var service = _serviceRepository.GetById(id);
            if (service == null || service.UserId != sellerId)
                return false;

            service.Title = dto.Title;
            service.Description = dto.Description;
            service.Price = dto.Price;
            service.Category = dto.Category;
            service.UpdatedAt = DateTime.UtcNow;

            _serviceRepository.Update(service);
            await _serviceRepository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> PatchServiceAsync(int id, ServicePatchDto dto, int sellerId)
        {
            var service = _serviceRepository.GetById(id);
            if (service == null || service.UserId != sellerId)
                return false;

            if (!string.IsNullOrEmpty(dto.Title)) service.Title = dto.Title;
            if (dto.Description != null) service.Description = dto.Description;
            if (dto.Price.HasValue) service.Price = dto.Price.Value;
            if (dto.Category != null) service.Category = dto.Category;
            service.UpdatedAt = DateTime.UtcNow;

            _serviceRepository.Update(service);
            await _serviceRepository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteServiceAsync(int id, int sellerId)
        {
            var service = _serviceRepository.GetById(id);
            if (service == null || service.UserId != sellerId)
                return false;

            // Delete image files from disk
            foreach (var img in service.Images)
            {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), img.ImageUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                if (File.Exists(filePath))
                {
                    try { File.Delete(filePath); } catch { /* log or ignore */ }
                }
            }

            _serviceRepository.Delete(service);
            await _serviceRepository.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<ServiceDto>> GetSellerServicesAsync(int sellerId)
        {
            var services = await _serviceRepository.Query()
                .Include(s => s.User)
                .Include(s => s.Images)
                .Where(s => s.UserId == sellerId)
                .ToListAsync();

            return services.Select(MapToDto);
        }

        private static ServiceDto MapToDto(Service s) => new ServiceDto
        {
            Id = s.Id,
            Title = s.Title,
            Description = s.Description,
            Price = s.Price,
            Category = s.Category,
            CreatedAt = s.CreatedAt,
            SellerId = s.UserId,
            SellerUsername = s.User?.UserName ?? "",
            Images = s.Images?.Select(img => new ServiceImageDto
            {
                Id = img.Id,
                ImageUrl = img.ImageUrl
            }).ToList() ?? new List<ServiceImageDto>()
        };

        public async Task<bool> DeleteServiceImageAsync(int serviceId, int imageId, int sellerId)
        {
            // Get the service to check ownership
            var service = await _context.Services
                .Include(s => s.Images)
                .FirstOrDefaultAsync(s => s.Id == serviceId && s.UserId == sellerId);

            if (service == null) return false;

            // Find and remove the image
            var image = service.Images.FirstOrDefault(img => img.Id == imageId);
            if (image == null) return false;

            // Delete from Cloudinary
            try
            {
                // Extract the public ID from the image URL
                // Cloudinary URLs typically look like: https://res.cloudinary.com/your-cloud-name/image/upload/v1234567890/service-images/abcdefg.jpg
                // The public ID in this case would be "service-images/abcdefg"
                string publicId = ExtractPublicIdFromUrl(image.ImageUrl);

                if (!string.IsNullOrEmpty(publicId))
                {
                    // Create deletion parameters
                    var deleteParams = new DeletionParams(publicId);

                    // Execute the deletion
                    var result = await _cloudinary.DestroyAsync(deleteParams);

                    // Log the result
                    if (result.Result == "ok")
                    {
                        // Successful deletion
                        Console.WriteLine($"Successfully deleted image with public ID: {publicId}");
                    }
                    else
                    {
                        // Failed deletion
                        Console.WriteLine($"Failed to delete image with public ID: {publicId}. Error: {result.Error?.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the error but continue with database deletion
                Console.WriteLine($"Error deleting image from Cloudinary: {ex.Message}");
            }

            // Remove from database
            _context.ServiceImages.Remove(image);
            await _context.SaveChangesAsync();

            return true;
        }

        private string ExtractPublicIdFromUrl(string imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl))
                return string.Empty;

            try
            {
                // Cloudinary URLs typically look like:
                // https://res.cloudinary.com/your-cloud-name/image/upload/v1234567890/folder/filename.extension

                // Extract everything after the /upload/ part
                int uploadIndex = imageUrl.IndexOf("/upload/");
                if (uploadIndex == -1)
                    return string.Empty;

                string afterUpload = imageUrl.Substring(uploadIndex + 8); // +8 for "/upload/"

                // Remove version number if present (v1234567890/)
                if (afterUpload.StartsWith("v") && afterUpload.Length > 10)
                {
                    int slashIndex = afterUpload.IndexOf('/');
                    if (slashIndex != -1 && slashIndex < 15) // Reasonable version number length
                    {
                        afterUpload = afterUpload.Substring(slashIndex + 1);
                    }
                }

                // Remove file extension
                int extensionIndex = afterUpload.LastIndexOf('.');
                if (extensionIndex != -1)
                {
                    afterUpload = afterUpload.Substring(0, extensionIndex);
                }

                return afterUpload;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error extracting public ID: {ex.Message}");
                return string.Empty;
            }
        }
    }
}
