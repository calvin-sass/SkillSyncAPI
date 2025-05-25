namespace SkillSyncAPI.Domain.DTOs.Bookings
{
    public class BookingDto
    {
        public int Id { get; set; }

        public DateTime BookingDate { get; set; }

        public string Status { get; set; }

        public int ServiceId { get; set; }

        public int UserId { get; set; }
    }
}
