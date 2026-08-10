namespace JobSeeker.Models.ViewModels.Admin
{
    public class JobApprovalViewModel
    {
        public List<JobApprovalItem> Jobs { get; set; } = new();
        public string? SearchKeyword { get; set; }
        public string? FilterEmploymentType { get; set; }
        public string? FilterJobStatus { get; set; }
        public int TotalCount { get; set; }
    }

    public class JobApprovalItem
    {
        public long JobId { get; set; }
        public string JobTitle { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string EmploymentType { get; set; } = string.Empty;
        public string? WorkplaceType { get; set; }
        public string? Location { get; set; }
        public decimal? MinimumSalary { get; set; }
        public decimal? MaximumSalary { get; set; }
        public DateOnly? ApplicationDeadline { get; set; }
        public string ApprovalStatus { get; set; } = string.Empty;
        public string JobStatus { get; set; } = string.Empty;
        public string? RejectionReason { get; set; }
        public string? ApprovedByName { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        // Detail fields
        public string? JobDescription { get; set; }
        public string? Responsibilities { get; set; }
        public string? MinimumQualification { get; set; }
        public string? PreferredFieldOfStudy { get; set; }
        public decimal MinimumExperienceYears { get; set; }
        public int VacancyCount { get; set; }
        public List<long> VacancyImageIds { get; set; } = new();
        public bool IsReopenRequest { get; set; }
    }
}
