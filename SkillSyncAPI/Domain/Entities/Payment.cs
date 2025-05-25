namespace SkillSyncAPI.Domain.Entities
{
    public class Payment
    {
        public int Id { get; set; }

        public int BookingId { get; set; }

        public Booking Booking { get; set; }

        public decimal Amount { get; set; }

        public DateTime PaymentDate { get; set; }

        public string PaymentMethod { get; set; } // e.g. "Card", "PayPal"

        public string Status { get; set; } // e.g. "Paid", "Pending", "Refunded"
    }
}
