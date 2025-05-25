using Microsoft.EntityFrameworkCore;
using SkillSyncAPI.Data;
using SkillSyncAPI.Domain.DTOs.Payments;
using SkillSyncAPI.Domain.Entities;
using SkillSyncAPI.Repositories;
using Stripe;

namespace SkillSyncAPI.Services.Impl
{
    public class PaymentService : IPaymentService
    {
        private readonly ApplicationDbContext _context;
        private readonly IPaymentRepository _paymentRepo;
        private readonly INotificationService _notificationService;
        private readonly IEmailService _emailService;

        public PaymentService(
            ApplicationDbContext context,
            IPaymentRepository paymentRepo,
            INotificationService notificationService,
            IEmailService emailService)
        {
            _context = context;
            _paymentRepo = paymentRepo;
            _notificationService = notificationService;
            _emailService = emailService;
        }

        public async Task<(bool Success, string? ErrorMessage)> ProcessStripePaymentAsync(int userId, PaymentCreateDto dto)
        {
            // Validate booking
            var booking = await _context.Bookings
                .Include(b => b.Service)
                .Include(b => b.User)
                .FirstOrDefaultAsync(b => b.Id == dto.BookingId && b.UserId == userId);

            if (booking == null)
                return (false, "Booking not found or not authorized.");

            if (booking.Status == "Paid")
                return (false, "Booking already paid.");

            // Set the amount from the service price
            dto.Amount = booking.Service.Price;

            try
            {
                // Create payment intent options
                var paymentIntentOptions = new PaymentIntentCreateOptions
                {
                    Amount = (long)(dto.Amount * 100), // Amount in cents
                    Currency = "zar",
                    PaymentMethod = dto.PaymentMethodId,
                    Description = $"Payment for booking #{booking.Id}",
                    Confirm = true,
                    ConfirmationMethod = "automatic"
                };

                // Choose either ConfirmationMethod OR AutomaticPaymentMethods, but not both
                if (dto.DisableRedirectPayments == true)
                {
                    // Don't set ConfirmationMethod when using AutomaticPaymentMethods
                    paymentIntentOptions.AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
                    {
                        Enabled = true,
                        AllowRedirects = "never"
                    };
                }
                else
                {
                    // Only use ConfirmationMethod when not using AutomaticPaymentMethods
                    paymentIntentOptions.ConfirmationMethod = "automatic";

                    if (!string.IsNullOrEmpty(dto.ReturnUrl))
                    {
                        paymentIntentOptions.ReturnUrl = dto.ReturnUrl;
                    }
                }

                // Create and confirm the payment intent with the configured options
                var paymentIntentService = new PaymentIntentService();
                var paymentIntent = await paymentIntentService.CreateAsync(paymentIntentOptions);

                if (paymentIntent.Status == "succeeded")
                {
                    // Record the payment
                    var payment = new Payment
                    {
                        BookingId = booking.Id,
                        Amount = dto.Amount,
                        PaymentDate = DateTime.UtcNow,
                        PaymentMethod = "Stripe",
                        Status = "Paid"
                    };
                    _paymentRepo.Add(payment);
                    booking.Status = "Paid";
                    _context.Bookings.Update(booking);
                    await _paymentRepo.SaveChangesAsync();

                    // Send notification to seller
                    if (booking.Service.UserId > 0)
                    {
                        await _notificationService.SendAsync(
                            booking.Service.UserId,
                            $"New payment received for booking #{booking.Id}"
                        );
                    }

                    // Send email confirmation to user
                    await _emailService.SendAsync(
                        booking.User.Email,
                        "Payment Confirmation",
                        $"Thank you for your payment of {dto.Amount:C} for booking #{booking.Id}."
                    );

                    return (true, null);
                }
                else
                {
                    return (false, $"Payment failed. Status: {paymentIntent.Status}");
                }
            }
            catch (StripeException ex)
            {
                // More detailed error handling for Stripe exceptions
                if (ex.StripeError?.Type == "invalid_request_error" &&
                    ex.Message.Contains("return_url"))
                {
                    return (false, "Payment configuration error: Return URL is required for this payment method");
                }

                // Log the error safely without exposing sensitive details
                return (false, $"Payment processing error: {ex.Message}");
            }
            catch (Exception ex)
            {
                // Log general exception
                return (false, "An unexpected error occurred. Please try again.");
            }
        }
    }
}
