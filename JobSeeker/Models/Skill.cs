using System.ComponentModel.DataAnnotations;

namespace JobSeeker.Models
{
    public class Skill
    {
        [Key]
        public long SkillId { get; set; }

        [Required]
        [StringLength(150)]
        public string SkillName { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public ICollection<JobSeekerSkill> JobSeekerSkills { get; set; } = new List<JobSeekerSkill>();
    }
}