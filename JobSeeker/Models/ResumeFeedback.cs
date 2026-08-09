using System.ComponentModel.DataAnnotations;

namespace JobSeeker.Models;

public class ResumeFeedback
{
    public long ResumeFeedbackId { get; set; }
    public long ResumeId { get; set; }
    [StringLength(450)] public string? CounsellorId { get; set; }
    public string? OverallComment { get; set; }
    public string? RequestMessage { get; set; }
    public string? Strengths { get; set; }
    public string? Weaknesses { get; set; }
    public string? RecommendedChanges { get; set; }
    [StringLength(30)] public string FeedbackStatus { get; set; } = "NEW";
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public Resume Resume { get; set; } = null!;
    public ApplicationUser? Counsellor { get; set; }
}
