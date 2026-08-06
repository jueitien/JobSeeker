using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JobSeeker.Models
{
    public class JobSeekerProfile
    {
        [Key]
        [StringLength(450)]
        public string JobSeekerId { get; set; } = string.Empty;

        [StringLength(4000)]
        public string? ProfileDescription { get; set; }

        [StringLength(4000)]
        public string? CareerObjective { get; set; }

        [StringLength(150)]
        public string? HighestQualification { get; set; }

        [StringLength(150)]
        public string? FieldOfStudy { get; set; }

        [StringLength(200)]
        public string? UniversityName { get; set; }

        public int? GraduationYear { get; set; }

        [StringLength(4000)]
        public string? ExperienceDescription { get; set; }

        [StringLength(200)]
        public string? PreferredJobTitle { get; set; }

        [StringLength(150)]
        public string? PreferredLocation { get; set; }

        [Column(TypeName = "decimal(12,2)")]
        public decimal? ExpectedSalary { get; set; }

        public DateOnly? AvailabilityDate { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public ApplicationUser User { get; set; } = null!;

        public ICollection<Resume> Resumes { get; set; } = new List<Resume>();

        public ICollection<Certification> Certifications { get; set; } = new List<Certification>();

        public ICollection<JobSeekerSkill> JobSeekerSkills { get; set; } = new List<JobSeekerSkill>();
    }
}