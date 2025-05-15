namespace SkillSyncAPI.DTOs.Services
{
    public class ServiceDto
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public string? Description { get; set; }

        public decimal Price { get; set; }

        public string? Category { get; set; }

        public int UserId { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
