using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace JobSeeker.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        public JobSeekerProfile? JobSeekerProfile { get; set; }

        public CompanyDetail? CompanyDetail { get; set; }
    }
}