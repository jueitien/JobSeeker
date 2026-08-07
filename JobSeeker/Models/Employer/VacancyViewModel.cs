using System.ComponentModel.DataAnnotations;

namespace JobSeeker.Models.Employer
{
    public class VacancyViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Job title is required.")]
        [StringLength(150)]
        [Display(Name = "Job Title")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Company name is required.")]
        [StringLength(150)]
        [Display(Name = "Company Name")]
        public string Company { get; set; } = string.Empty;

        [Required(ErrorMessage = "Location is required.")]
        [StringLength(150)]
        public string Location { get; set; } = string.Empty;

        [Required(ErrorMessage = "Job type is required.")]
        [Display(Name = "Job Type")]
        public string JobType { get; set; } = string.Empty;

        [Required(ErrorMessage = "Experience level is required.")]
        [Display(Name = "Experience Level")]
        public string ExperienceLevel { get; set; } = string.Empty;

        [Display(Name = "Salary Range (optional)")]
        [StringLength(80)]
        public string? SalaryRange { get; set; }

        [Required(ErrorMessage = "Job description is required.")]
        [Display(Name = "Job Description")]
        [StringLength(5000)]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Requirements are required.")]
        [StringLength(3000)]
        public string Requirements { get; set; } = string.Empty;

        [Display(Name = "Application Deadline")]
        [DataType(DataType.Date)]
        public DateTime? Deadline { get; set; }

        [Display(Name = "Posted On")]
        public DateTime PostedOn { get; set; } = DateTime.Now;

        public string PostedBy { get; set; } = string.Empty;
    }

    public class VacanciesPageViewModel
    {
        public VacancyViewModel NewVacancy { get; set; } = new();
        public List<VacancyViewModel> PublishedVacancies { get; set; } = new();

        public string? SearchKeyword { get; set; }
        public string? FilterJobType { get; set; }
        public string? FilterExperienceLevel { get; set; }
        public string? FilterLocation { get; set; }
    }
}
