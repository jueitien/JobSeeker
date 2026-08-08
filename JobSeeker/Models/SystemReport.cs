using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JobSeeker.Models
{
    public class SystemReport
    {
        [Key]
        public long SystemReportId { get; set; }

        // FK to admin user who generated the report
        [Required, StringLength(450)]
        public string GeneratedBy { get; set; } = string.Empty;

        [Required, StringLength(200)]
        public string ReportName { get; set; } = string.Empty;

        // e.g. USER_SUMMARY, JOB_SUMMARY, APPLICATION_SUMMARY
        [Required, StringLength(100)]
        public string ReportType { get; set; } = string.Empty;

        // JSON string storing filter parameters used
        public string? ReportParameters { get; set; }

        [StringLength(255)]
        public string? OriginalFileName { get; set; }

        // S3 key if report was exported as file
        [StringLength(1024)]
        public string? ReportS3Key { get; set; }

        [StringLength(100)]
        public string? FileContentType { get; set; }

        public long? FileSizeBytes { get; set; }

        public DateTime GeneratedAt { get; set; }

        // Navigation property
        public ApplicationUser Generator { get; set; } = null!;
    }
}
