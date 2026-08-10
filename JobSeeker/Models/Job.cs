using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JobSeeker.Models
{
    public class Job
    {
        [Key]
        public long JobId { get; set; }

        [StringLength(450)]
        public string? EmployerId { get; set; }

        [Required, StringLength(200)]
        public string CompanyName { get; set; } = string.Empty;

        [Required, StringLength(200)]
        public string JobTitle { get; set; } = string.Empty;

        [Required]
        public string JobDescription { get; set; } = string.Empty;

        public string? Responsibilities { get; set; }

        [StringLength(150)]
        public string? MinimumQualification { get; set; }

        [StringLength(150)]
        public string? PreferredFieldOfStudy { get; set; }

        [Column(TypeName = "decimal(4,1)")]
        public decimal MinimumExperienceYears { get; set; }

        [Required, StringLength(50)]
        public string EmploymentType { get; set; } = "FULL_TIME";

        [StringLength(30)]
        public string? WorkplaceType { get; set; }

        [StringLength(200)]
        public string? Location { get; set; }

        [Column(TypeName = "decimal(12,2)")]
        public decimal? MinimumSalary { get; set; }

        [Column(TypeName = "decimal(12,2)")]
        public decimal? MaximumSalary { get; set; }

        public int VacancyCount { get; set; } = 1;

        public DateOnly? ApplicationDeadline { get; set; }

        [StringLength(30)]
        public string ApprovalStatus { get; set; } = "APPROVED";

        [StringLength(30)]
        public string JobStatus { get; set; } = "OPEN";

        public string? RejectionReason { get; set; }

        [StringLength(450)]
        public string? ApprovedBy { get; set; }

        public DateTime? ApprovedAt { get; set; }

        public bool IsTestData { get; set; }

        public bool IsReopenRequest { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public ApplicationUser? Employer { get; set; }

        public ApplicationUser? Approver { get; set; }

        public ICollection<JobRequiredSkill> RequiredSkills { get; set; } = new List<JobRequiredSkill>();

        public ICollection<JobApplication> Applications { get; set; } = new List<JobApplication>();

        public ICollection<JobVacancyImage> VacancyImages { get; set; } = new List<JobVacancyImage>();
    }
}
