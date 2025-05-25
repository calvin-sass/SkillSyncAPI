using SkillSyncAPI.Domain.DTOs.Payments;

namespace SkillSyncAPI.Services
{
    public interface IPaymentService
    {
        Task<(bool Success, string? ErrorMessage)> ProcessStripePaymentAsync(int userId, PaymentCreateDto dto);
    }
}
