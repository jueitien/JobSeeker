using System;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace JobSeeker.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [StringLength(1024)]
        public string? ProfileImageS3Key { get; set; }

        [Required]
        [StringLength(30)]
        public string AccountStatus { get; set; } = "ACTIVE";

        public DateTime? LastLoginAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        public JobSeekerProfile? JobSeekerProfile { get; set; }

        public CompanyDetail? CompanyDetail { get; set; }
    }
}
