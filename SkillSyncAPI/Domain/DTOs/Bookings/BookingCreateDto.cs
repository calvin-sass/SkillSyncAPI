using System.ComponentModel.DataAnnotations;

namespace SkillSyncAPI.Domain.DTOs.Bookings
{
    public class BookingCreateDto
    {
        [Required]
        public int ServiceId { get; set; }

        [Required]
        public DateTime BookingDate { get; set; }
    }
}
