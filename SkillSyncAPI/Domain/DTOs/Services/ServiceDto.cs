namespace SkillSyncAPI.Domain.DTOs.Services
{
    public class ServiceDto
    {
        public int Id { get; set; }

        public List<ServiceImageDto> Images { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public decimal Price { get; set; }

        public string Category { get; set; }

        public DateTime CreatedAt { get; set; }

        // Seller info
        public int SellerId { get; set; }

        public string SellerUsername { get; set; }
    }
}
