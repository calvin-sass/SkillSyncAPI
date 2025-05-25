using SkillSyncAPI.Domain.DTOs.Services;

namespace SkillSyncAPI.Services
{
    public interface IServiceService
    {
        Task<ServiceDto?> GetServiceByIdAsync(int id);

        Task<IEnumerable<ServiceDto>> GetFilteredServicesAsync(string? category, string? priceRange, string? search);

        Task<ServiceDto> CreateServiceAsync(ServiceCreateDto dto, int sellerId);

        Task<(bool Success, string? ImageUrl, string? Error)> UploadServiceImageAsync(int serviceId, int sellerId, IFormFile image);

        Task<bool> UpdateServiceAsync(int id, ServiceUpdateDto dto, int sellerId);

        Task<bool> PatchServiceAsync(int id, ServicePatchDto dto, int sellerId);

        Task<bool> DeleteServiceAsync(int id, int sellerId);

        Task<IEnumerable<ServiceDto>> GetSellerServicesAsync(int sellerId);

        Task<bool> DeleteServiceImageAsync(int serviceId, int imageId, int sellerId);
    }
}
