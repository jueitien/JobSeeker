using System.ComponentModel.DataAnnotations;

namespace JobSeeker.Models;

public class SkillRecommendation
{
    public long SkillRecommendationId { get; set; }
    [StringLength(450)] public string JobSeekerId { get; set; } = string.Empty;
    [StringLength(450)] public string? CounsellorId { get; set; }
    public string? RequestMessage { get; set; }
    public string? RecommendedSkill { get; set; }
    public string? RecommendationReason { get; set; }
    [StringLength(20)] public string PriorityLevel { get; set; } = "MEDIUM";
    [StringLength(30)] public string RecommendationSource { get; set; } = "COUNSELLOR";
    [StringLength(30)] public string RecommendationStatus { get; set; } = "NEW";
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public JobSeekerProfile JobSeeker { get; set; } = null!;
    public ApplicationUser? Counsellor { get; set; }
}
