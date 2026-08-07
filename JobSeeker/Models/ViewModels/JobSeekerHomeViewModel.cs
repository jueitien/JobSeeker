namespace JobSeeker.Models.ViewModels
{
    public class JobSeekerHomeViewModel
    {
        public string? Keyword { get; set; }
        public string? Location { get; set; }
        public string? EmploymentType { get; set; }
        public string? WorkplaceType { get; set; }
        public decimal? MinimumSalary { get; set; }
        public string Sort { get; set; } = "match";

        public List<JobListingItemViewModel> Jobs { get; set; } = new();
        public List<string> AvailableLocations { get; set; } = new();
        public List<string> AvailableEmploymentTypes { get; set; } = new();
        public List<string> AvailableWorkplaceTypes { get; set; } = new();
    }

    public class JobListingItemViewModel
    {
        public Job Job { get; set; } = null!;
        public decimal MatchPercentage { get; set; }
        public int MatchedSkillCount { get; set; }
        public int RequiredSkillCount { get; set; }
        public List<string> MatchedSkills { get; set; } = new();
        public List<string> MissingSkills { get; set; } = new();
        public bool HasApplied { get; set; }
        public string? ApplicationStatus { get; set; }
    }
}
