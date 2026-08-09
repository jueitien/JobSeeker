namespace JobSeeker.Models.ViewModels
{
    public class FeedbackHubViewModel
    {
        public List<Resume> Resumes { get; set; } = new();
        public List<FeedbackRequestItemViewModel> Requests { get; set; } = new();
        public List<TrendingSkillViewModel> TrendingSkills { get; set; } = new();
    }

    public class FeedbackRequestItemViewModel
    {
        public long Id { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? RequestMessage { get; set; }
        public string Status { get; set; } = "NEW";
        public DateTime CreatedAt { get; set; }
        public string? CounsellorName { get; set; }
        public string? ResponseTitle { get; set; }
        public string? ResponseBody { get; set; }
        public string? ResponseDetails { get; set; }
    }

    public class TrendingSkillViewModel
    {
        public long SkillId { get; set; }
        public string SkillName { get; set; } = string.Empty;
        public int JobCount { get; set; }
        public bool AlreadyAdded { get; set; }
    }
}
