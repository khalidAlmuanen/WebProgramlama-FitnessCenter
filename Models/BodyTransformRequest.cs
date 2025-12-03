using System;
using FitnessCenterApp.Models;

namespace FitnessCenterApp.Models
{
    public class BodyTransformRequest
    {
        public int Id { get; set; }

        public string MemberId { get; set; } = null!;
        public ApplicationUser Member { get; set; } = null!;

        public string GoalType { get; set; } = string.Empty;
        public int DurationMonths { get; set; }

        public double? StartWeightKg { get; set; }
        public double? ExpectedChangePercent { get; set; }

        public string OriginalImagePath { get; set; } = string.Empty;
        public string GeneratedImagePath { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
