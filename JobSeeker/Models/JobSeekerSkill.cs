using System.ComponentModel.DataAnnotations;

namespace JobSeeker.Models
{
    public class JobSeekerSkill
    {
        [Key]
        public long JobSeekerSkillId { get; set; }

        [StringLength(450)]
        public string JobSeekerId { get; set; } = string.Empty;

        public long SkillId { get; set; }

        [StringLength(30)]
        public string? ProficiencyLevel { get; set; }

        public DateTime CreatedAt { get; set; }

        public JobSeekerProfile JobSeekerProfile { get; set; } = null!;

        public Skill Skill { get; set; } = null!;
    }
}