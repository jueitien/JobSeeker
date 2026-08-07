using System.ComponentModel.DataAnnotations;

namespace JobSeeker.Models.ViewModels
{
    public class EditAccountViewModel
    {
        [Required]
        [StringLength(100)]
        [Display(Name = "Full name")]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [Display(Name = "Email address")]
        public string Email { get; set; } = string.Empty;

        [Phone]
        [Display(Name = "Phone number")]
        public string? PhoneNumber { get; set; }
    }
}
