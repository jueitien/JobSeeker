using System.ComponentModel.DataAnnotations;

namespace JobSeeker.Models
{
    /// <summary>
    /// Stores an Employer's company profile (name + address).
    /// One Employer (AspNetUsers row) has at most one CompanyDetail.
    /// Used to auto-fill the "Post a Vacancy" form.
    /// </summary>
    public class CompanyDetail
    {
        [Key]
        public long CompanyDetailId { get; set; }

        [Required]
        [StringLength(450)]
        public string EmployerId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Company name is required.")]
        [StringLength(200)]
        [Display(Name = "Company Name")]
        public string CompanyName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Company address is required.")]
        [StringLength(300)]
        [Display(Name = "Company Address")]
        public string Address { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public ApplicationUser Employer { get; set; } = null!;
    }
}
