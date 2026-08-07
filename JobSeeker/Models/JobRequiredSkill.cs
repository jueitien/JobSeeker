using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JobSeeker.Models
{
    public class JobRequiredSkill
    {
        [Key]
        public long JobRequiredSkillId { get; set; }

        public long JobId { get; set; }

        public long SkillId { get; set; }

        [StringLength(30)]
        public string RequirementType { get; set; } = "REQUIRED";

        [Column(TypeName = "decimal(5,2)")]
        public decimal ImportanceWeight { get; set; }

        public Job Job { get; set; } = null!;

        public Skill Skill { get; set; } = null!;
    }
}
