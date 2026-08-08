namespace JobSeeker.Models.ViewModels.Admin
{
    public class EmployerVerificationViewModel
    {
        public List<EmployerVerificationItem> Employers { get; set; } = new();
        public string? SearchKeyword { get; set; }
        public string? FilterIndustry { get; set; }
        public int TotalCount { get; set; }
    }

    public class EmployerVerificationItem
    {
        public string EmployerId { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Industry { get; set; }
        public string? CompanySize { get; set; }
        public string? CompanyRegistrationNumber { get; set; }
        public string VerificationStatus { get; set; } = string.Empty;
        public string? VerificationRemarks { get; set; }
        public string? VerifiedByName { get; set; }
        public DateTime? VerifiedAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
