namespace SkillSyncAPI.Domain.DTOs.Payments
{
    public class PaymentCreateDto
    {
        public int BookingId { get; set; }
        public string PaymentMethodId { get; set; }

        // This will be populated by the service
        public decimal Amount { get; set; }

        public string ReturnUrl { get; set; }

        public bool? DisableRedirectPayments { get; set; }
    }
}
