using Microsoft.EntityFrameworkCore;
using SkillSyncAPI.Domain.DTOs.Bookings;
using SkillSyncAPI.Domain.Entities;
using SkillSyncAPI.Repositories;

namespace SkillSyncAPI.Services.Impl
{
    public class BookingService : IBookingService
    {
        private readonly IBookingRepository _bookingRepo;
        private readonly IServiceRepository _serviceRepo;
        private readonly INotificationService _notificationService;

        public BookingService(
            IBookingRepository bookingRepo,
            IServiceRepository serviceRepo,
            INotificationService notificationService)
        {
            _bookingRepo = bookingRepo;
            _serviceRepo = serviceRepo;
            _notificationService = notificationService;
        }

        public async Task<Booking?> CreateBookingAsync(int userId, BookingCreateDto dto)
        {
            var service = await _serviceRepo.Query().FirstOrDefaultAsync(s => s.Id == dto.ServiceId);
            if (service == null) return null;

            var booking = new Booking
            {
                UserId = userId,
                ServiceId = dto.ServiceId,
                BookingDate = dto.BookingDate,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                ModifiedByRole = "User",
                LastModifiedByUserId = userId
            };

            _bookingRepo.Add(booking);
            await _bookingRepo.SaveChangesAsync();

            // Notify seller using NotificationService
            await _notificationService.SendAsync(
                service.UserId,
                $"New booking request for your service '{service.Title}'.");

            return booking;
        }

        public async Task<bool> UpdateBookingDateAsync(int bookingId, int sellerId, DateTime newDate)
        {
            var booking = await _bookingRepo.Query()
                .Include(b => b.Service)
                .FirstOrDefaultAsync(b => b.Id == bookingId);

            if (booking == null || booking.Service.UserId != sellerId)
                return false;

            // Only allow update if payment is made
            if (booking.Payment == null || booking.Payment.Status != "Paid")
                return false;

            booking.BookingDate = newDate;
            booking.UpdatedAt = DateTime.UtcNow;
            booking.ModifiedByRole = "Seller";
            booking.LastModifiedByUserId = sellerId;

            _bookingRepo.Update(booking);
            await _bookingRepo.SaveChangesAsync();

            // Notify user using NotificationService
            await _notificationService.SendAsync(
                booking.UserId,
                $"Your booking date for '{booking.Service.Title}' has been updated to {newDate:yyyy-MM-dd HH:mm}.");

            return true;
        }

        public async Task<List<BookingDto>> GetBookingsForUserAsync(int userId)
        {
            return await _bookingRepo.Query()
                .Where(b => b.UserId == userId)
                .Select(b => new BookingDto
                {
                    Id = b.Id,
                    BookingDate = b.BookingDate,
                    Status = b.Status,
                    ServiceId = b.ServiceId,
                    UserId = b.UserId
                })
                .ToListAsync();
        }

        public async Task<List<BookingDto>> GetBookingsForSellerAsync(int sellerId)
        {
            return await _bookingRepo.Query()
                .Include(b => b.Service)
                .Where(b => b.Service.UserId == sellerId)
                .Select(b => new BookingDto
                {
                    Id = b.Id,
                    BookingDate = b.BookingDate,
                    Status = b.Status,
                    ServiceId = b.ServiceId,
                    UserId = b.UserId
                })
                .ToListAsync();
        }

        public async Task<bool> CancelBookingAsync(int bookingId, int userId, bool isSeller)
        {
            var booking = await _bookingRepo.Query()
                .Include(b => b.Service)
                .FirstOrDefaultAsync(b => b.Id == bookingId);

            if (booking == null)
                return false;

            // Only the user who made the booking or the seller who owns the service can cancel
            if (!isSeller && booking.UserId != userId)
                return false;
            if (isSeller && booking.Service.UserId != userId)
                return false;

            booking.Status = "Cancelled";
            booking.UpdatedAt = DateTime.UtcNow;
            booking.ModifiedByRole = isSeller ? "Seller" : "User";
            booking.LastModifiedByUserId = userId;

            _bookingRepo.Update(booking);
            await _bookingRepo.SaveChangesAsync();

            // Notify the other party
            if (isSeller)
            {
                await _notificationService.SendAsync(
                    booking.UserId,
                    $"Your booking for '{booking.Service.Title}' was cancelled by the seller.");
            }
            else
            {
                await _notificationService.SendAsync(
                    booking.Service.UserId,
                    $"A user cancelled their booking for your service '{booking.Service.Title}'.");
            }

            return true;
        }
    }
}
