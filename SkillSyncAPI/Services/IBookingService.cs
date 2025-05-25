using SkillSyncAPI.Domain.DTOs.Bookings;
using SkillSyncAPI.Domain.Entities;

namespace SkillSyncAPI.Services
{
    public interface IBookingService
    {
        Task<Booking?> CreateBookingAsync(int userId, BookingCreateDto dto);

        Task<bool> UpdateBookingDateAsync(int bookingId, int sellerId, DateTime newDate);

        Task<List<BookingDto>> GetBookingsForUserAsync(int userId);

        Task<List<BookingDto>> GetBookingsForSellerAsync(int sellerId);

        Task<bool> CancelBookingAsync(int bookingId, int userId, bool isSeller);
    }
}
