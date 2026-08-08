using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JobSeeker.Models
{
    public class AuditLog
    {
        [Key]
        public long AuditLogId { get; set; }

        // Nullable — user might be deleted but log stays
        [StringLength(450)]
        public string? UserId { get; set; }

        // e.g. USER_SUSPENDED, EMPLOYER_APPROVED, JOB_REJECTED
        [Required, StringLength(100)]
        public string ActionType { get; set; } = string.Empty;

        // e.g. User, Job, EmployerProfile
        [StringLength(100)]
        public string? EntityType { get; set; }

        // The ID of the affected record
        public long? EntityId { get; set; }

        public string? ActionDescription { get; set; }

        [StringLength(45)]
        public string? IpAddress { get; set; }

        public DateTime CreatedAt { get; set; }

        // Navigation property
        public ApplicationUser? User { get; set; }
    }
}
