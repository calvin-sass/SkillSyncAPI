namespace SkillSyncAPI.Domain.Entities
{
    public class Booking
    {
        public int Id { get; set; }

        public DateTime BookingDate { get; set; }

        public string Status { get; set; } = "Pending"; // Pending, Accepted, Rejected, Completed

        public Payment Payment { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public int? LastModifiedByUserId { get; set; } // Who changed it last (could be user or seller)

        public string? ModifiedByRole { get; set; }    // "User" or "Seller"

        //Foreign Keys
        public int UserId { get; set; }

        public ApplicationUser User { get; set; }

        public int ServiceId { get; set; }

        public Service Service { get; set; }
    }
}
