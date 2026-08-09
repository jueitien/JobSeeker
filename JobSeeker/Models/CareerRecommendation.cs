using System.ComponentModel.DataAnnotations;

namespace JobSeeker.Models;

public class CareerRecommendation
{
    public long CareerRecommendationId { get; set; }
    [StringLength(450)] public string JobSeekerId { get; set; } = string.Empty;
    [StringLength(450)] public string? CounsellorId { get; set; }
    public string? RequestMessage { get; set; }
    [Required, StringLength(200)] public string RecommendedJobTitle { get; set; } = string.Empty;
    [StringLength(150)] public string? RecommendedIndustry { get; set; }
    [Required] public string RecommendationReason { get; set; } = string.Empty;
    public string? RequiredImprovements { get; set; }
    [StringLength(30)] public string RecommendationSource { get; set; } = "COUNSELLOR";
    [StringLength(30)] public string RecommendationStatus { get; set; } = "NEW";
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public JobSeekerProfile JobSeeker { get; set; } = null!;
    public ApplicationUser? Counsellor { get; set; }
}
