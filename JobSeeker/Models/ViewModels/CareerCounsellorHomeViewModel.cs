namespace JobSeeker.Models.ViewModels;

public class CareerCounsellorHomeViewModel
{
    public int ResumeReviewCount { get; init; }
    public int CareerRecommendationCount { get; init; }
    public int SkillRecommendationCount { get; init; }
    public int CertificationRecommendationCount { get; init; }
    public IReadOnlyList<RecentResumeReviewViewModel> RecentResumeReviews { get; init; }
        = [];
}

public class RecentResumeReviewViewModel
{
    public required string JobSeekerName { get; init; }
    public required string ResumeTitle { get; init; }
    public required string FeedbackStatus { get; init; }
    public DateTime UpdatedAt { get; init; }
}
