using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JobSeeker.Models
{
    public class EmployerProfile
    {
        // Same ID as the ApplicationUser (1-to-1)
        [Key]
        [StringLength(450)]
        public string EmployerId { get; set; } = string.Empty;

        [Required, StringLength(200)]
        public string CompanyName { get; set; } = string.Empty;

        [StringLength(100)]
        public string? CompanyRegistrationNumber { get; set; }

        [StringLength(150)]
        public string? Industry { get; set; }

        [StringLength(50)]
        public string? CompanySize { get; set; }

        public string? CompanyDescription { get; set; }

        public string? CompanyWebsite { get; set; }

        public string? CompanyAddress { get; set; }

        [StringLength(1024)]
        public string? CompanyLogoS3Key { get; set; }

        // PENDING / APPROVED / REJECTED
        [Required, StringLength(30)]
        public string VerificationStatus { get; set; } = "PENDING";

        public string? VerificationRemarks { get; set; }

        // FK to the admin user who verified
        [StringLength(450)]
        public string? VerifiedBy { get; set; }

        public DateTime? VerifiedAt { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        // Navigation properties
        public ApplicationUser User { get; set; } = null!;

        public ApplicationUser? Verifier { get; set; }
    }
}
