namespace SkillSyncAPI.Domain.DTOs.Reviews
{
    public class ReviewUpdateDto
    {
        public decimal Rating { get; set; }

        public string? Comment { get; set; }
    }
}
