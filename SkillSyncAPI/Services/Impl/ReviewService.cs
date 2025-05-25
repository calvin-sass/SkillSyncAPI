using SkillSyncAPI.Domain.DTOs.Reviews;
using SkillSyncAPI.Domain.Entities;
using SkillSyncAPI.Repositories;
using Microsoft.EntityFrameworkCore;
using SkillSyncAPI.Services;

public class ReviewService : IReviewService
{
    private readonly IReviewRepository _reviewRepo;
    private readonly IBookingRepository _bookingRepo;
    private readonly IServiceRepository _serviceRepo;
    private readonly INotificationService _notificationService;

    public ReviewService(
        IReviewRepository reviewRepo,
        IBookingRepository bookingRepo,
        IServiceRepository serviceRepo,
        INotificationService notificationService)
    {
        _reviewRepo = reviewRepo;
        _bookingRepo = bookingRepo;
        _serviceRepo = serviceRepo;
        _notificationService = notificationService;
    }

    public async Task<bool> CanUserReviewServiceAsync(int userId, int serviceId)
    {
        return await _bookingRepo.Query()
            .AnyAsync(b => b.UserId == userId && b.ServiceId == serviceId && b.Payment != null && b.Payment.Status == "Paid");
    }

    public async Task<ReviewDto?> CreateReviewAsync(int userId, ReviewCreateDto dto)
    {
        // Validate booking
        var booking = await _bookingRepo.Query()
            .Include(b => b.Service)
            .FirstOrDefaultAsync(b => b.Id == dto.BookingId && b.UserId == userId && b.ServiceId == dto.ServiceId && b.Payment != null && b.Payment.Status == "Paid");
        if (booking == null)
            return null;

        // Prevent duplicate review for the same booking
        if (await _reviewRepo.Query().AnyAsync(r => r.UserId == userId && r.ServiceId == dto.ServiceId && r.BookingId == dto.BookingId))
            return null;

        var now = DateTime.UtcNow;
        var review = new Review
        {
            UserId = userId,
            ServiceId = dto.ServiceId,
            BookingId = dto.BookingId,
            Rating = dto.Rating,
            Comment = dto.Comment,
            CreatedAt = now,
            UpdatedAt = now,
            IsDeleted = false // Ensure new reviews are not soft-deleted
        };
        _reviewRepo.Add(review);
        await _reviewRepo.SaveChangesAsync();

        // Notify the seller
        var service = booking.Service;
        if (service != null)
        {
            await _notificationService.SendAsync(
                service.UserId,
                $"Your service '{service.Title}' received a new review."
            );
        }

        return new ReviewDto
        {
            Id = review.Id,
            UserId = review.UserId,
            ServiceId = review.ServiceId,
            BookingId = review.BookingId,
            Rating = review.Rating,
            Comment = review.Comment,
            CreatedAt = review.CreatedAt,
            UpdatedAt = review.UpdatedAt
        };
    }

    public async Task<bool> UpdateReviewAsync(int userId, int reviewId, ReviewUpdateDto dto)
    {
        var review = await _reviewRepo.GetByIdAsync(reviewId);
        if (review == null || review.UserId != userId || review.IsDeleted)
            return false;

        review.Rating = dto.Rating;
        review.Comment = dto.Comment;
        review.UpdatedAt = DateTime.UtcNow;
        _reviewRepo.Update(review);
        await _reviewRepo.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteReviewAsync(int userId, int reviewId)
    {
        var review = await _reviewRepo.GetByIdAsync(reviewId);
        if (review == null || review.UserId != userId || review.IsDeleted)
            return false;

        // Soft delete: mark as deleted instead of removing from DB
        review.IsDeleted = true;
        review.UpdatedAt = DateTime.UtcNow;
        _reviewRepo.Update(review);
        await _reviewRepo.SaveChangesAsync();
        return true;
    }

    public async Task<List<ReviewDto>> GetReviewsForServiceAsync(int serviceId)
    {
        return await _reviewRepo.Query()
            .Where(r => r.ServiceId == serviceId && !r.IsDeleted)
            .Select(r => new ReviewDto
            {
                Id = r.Id,
                UserId = r.UserId,
                ServiceId = r.ServiceId,
                BookingId = r.BookingId,
                Rating = r.Rating,
                Comment = r.Comment,
                CreatedAt = r.CreatedAt,
                UpdatedAt = r.UpdatedAt
            }).ToListAsync();
    }

    public async Task<List<ReviewDto>> GetReviewsByUserAsync(int userId)
    {
        return await _reviewRepo.Query()
            .Where(r => r.UserId == userId && !r.IsDeleted)
            .Select(r => new ReviewDto
            {
                Id = r.Id,
                UserId = r.UserId,
                ServiceId = r.ServiceId,
                BookingId = r.BookingId,
                Rating = r.Rating,
                Comment = r.Comment,
                CreatedAt = r.CreatedAt,
                UpdatedAt = r.UpdatedAt
            }).ToListAsync();
    }
}