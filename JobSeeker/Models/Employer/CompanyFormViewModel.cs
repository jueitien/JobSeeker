using System.ComponentModel.DataAnnotations;

namespace JobSeeker.Models.Employer
{
    /// <summary>
    /// Form model for the Employer's "Manage Company" page, backed by the
    /// employer_profiles table (EmployerProfile entity). Covers every
    /// editable column on that table except CompanyLogoS3Key (logo uploads
    /// are intentionally out of scope for this form). Verification status
    /// and remarks are shown read-only — they're set by an Administrator via
    /// the Employer Verification workflow.
    /// </summary>
    public class CompanyFormViewModel
    {
        [Required(ErrorMessage = "Company name is required.")]
        [StringLength(200)]
        [Display(Name = "Company Name")]
        public string CompanyName { get; set; } = string.Empty;

        [StringLength(100)]
        [Display(Name = "Company Registration Number")]
        public string? CompanyRegistrationNumber { get; set; }

        [StringLength(150)]
        [Display(Name = "Industry")]
        public string? Industry { get; set; }

        [StringLength(50)]
        [Display(Name = "Company Size")]
        public string? CompanySize { get; set; }

        [Display(Name = "Company Description")]
        public string? CompanyDescription { get; set; }

        [Url(ErrorMessage = "Enter a valid website URL (e.g. https://example.com).")]
        [Display(Name = "Company Website")]
        public string? CompanyWebsite { get; set; }

        [Required(ErrorMessage = "Company address is required.")]
        [Display(Name = "Company Address")]
        public string CompanyAddress { get; set; } = string.Empty;

        /// <summary>Read-only. PENDING / APPROVED / REJECTED — set by an Administrator.</summary>
        public string VerificationStatus { get; set; } = "PENDING";

        /// <summary>Read-only. Populated by an Administrator, typically when rejecting.</summary>
        public string? VerificationRemarks { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public bool IsNew { get; set; }
    }
}
