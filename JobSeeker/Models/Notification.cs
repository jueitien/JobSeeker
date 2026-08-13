using System.ComponentModel.DataAnnotations;

namespace JobSeeker.Models
{
    public class Notification
    {
        [Key]
        public long NotificationId { get; set; }

        [Required, StringLength(450)]
        public string UserId { get; set; } = string.Empty;

        [Required, StringLength(50)]
        public string NotificationType { get; set; } = string.Empty;

        [Required, StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Message { get; set; } = string.Empty;

        [StringLength(50)]
        public string? ReferenceType { get; set; }

        public long? ReferenceId { get; set; }

        public bool IsRead { get; set; }

        public DateTime? ReadAt { get; set; }

        public DateTime CreatedAt { get; set; }

        public ApplicationUser User { get; set; } = null!;
    }
}
