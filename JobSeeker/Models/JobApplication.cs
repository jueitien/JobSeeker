using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JobSeeker.Models
{
    public class JobApplication
    {
        [Key]
        public long ApplicationId { get; set; }

        public long JobId { get; set; }

        [Required, StringLength(450)]
        public string JobSeekerId { get; set; } = string.Empty;

        public long ResumeId { get; set; }

        public string? CoverLetter { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal? MatchPercentageAtApplication { get; set; }

        [Required, StringLength(30)]
        public string ApplicationStatus { get; set; } = "SUBMITTED";

        public string? EmployerNotes { get; set; }

        public DateTime AppliedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public Job Job { get; set; } = null!;

        public JobSeekerProfile JobSeekerProfile { get; set; } = null!;

        public Resume Resume { get; set; } = null!;
    }
}
