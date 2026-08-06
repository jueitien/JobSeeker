using System.ComponentModel.DataAnnotations;

namespace JobSeeker.Models
{
    public class Certification
    {
        [Key]
        public long CertificationId { get; set; }

        [StringLength(450)]
        public string JobSeekerId { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string CertificationName { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required]
        [StringLength(1024)]
        public string CertificateS3Key { get; set; } = string.Empty;

        public DateTime UploadedAt { get; set; }

        public JobSeekerProfile JobSeekerProfile { get; set; } = null!;
    }
}