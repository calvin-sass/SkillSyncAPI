﻿namespace SkillSyncAPI.Domain.DTOs.Services
{
    public class ServicePatchDto
    {
        public string? Title { get; set; }

        public string? Description { get; set; }

        public decimal? Price { get; set; }

        public string? Category { get; set; }
    }
}
