using System.ComponentModel.DataAnnotations;

namespace JobSeeker.Models
{
    public class Resume
    {
        [Key]
        public long ResumeId { get; set; }

        [StringLength(450)]
        public string JobSeekerId { get; set; } = string.Empty;

        [StringLength(200)]
        public string? ResumeTitle { get; set; }

        public string? ResumeDescription { get; set; }

        [Required]
        [StringLength(1024)]
        public string ResumeS3Key { get; set; } = string.Empty;

        public string? ExtractedText { get; set; }

        public bool IsPrimary { get; set; }

        public DateTime UploadedAt { get; set; }

        public JobSeekerProfile JobSeekerProfile { get; set; } = null!;

        public ICollection<JobApplication> JobApplications { get; set; } = new List<JobApplication>();

        public ICollection<ResumeFeedback> FeedbackRequests { get; set; } = new List<ResumeFeedback>();
    }
}
