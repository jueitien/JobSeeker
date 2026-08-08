using System.ComponentModel.DataAnnotations;

namespace JobSeeker.Models.Employer
{
    /// <summary>
    /// Form model used when an Employer creates a new job vacancy.
    /// Field names/values line up with the `jobs` table (see JobSeeker.Models.Job)
    /// so they can be mapped directly onto a Job entity in the controller.
    /// </summary>
    public class VacancyFormViewModel : IValidatableObject
    {
        [Required(ErrorMessage = "Job title is required.")]
        [StringLength(200)]
        [Display(Name = "Job Title")]
        public string JobTitle { get; set; } = string.Empty;

        [Required(ErrorMessage = "Company name is required.")]
        [StringLength(200)]
        [Display(Name = "Company Name")]
        public string CompanyName { get; set; } = string.Empty;

        [StringLength(200)]
        public string? Location { get; set; }

        [Required(ErrorMessage = "Employment type is required.")]
        [Display(Name = "Employment Type")]
        public string EmploymentType { get; set; } = string.Empty;   // FULL_TIME, PART_TIME, CONTRACT, INTERNSHIP

        [Display(Name = "Workplace Type")]
        public string? WorkplaceType { get; set; }                   // ON_SITE, HYBRID, REMOTE

        [StringLength(150)]
        [Display(Name = "Minimum Qualification")]
        public string? MinimumQualification { get; set; }

        [StringLength(150)]
        [Display(Name = "Preferred Field of Study")]
        public string? PreferredFieldOfStudy { get; set; }

        [Range(0, 50, ErrorMessage = "Enter a value between 0 and 50.")]
        [Display(Name = "Minimum Experience (years)")]
        public decimal MinimumExperienceYears { get; set; }

        [Range(0, 999999, ErrorMessage = "Enter a valid salary amount.")]
        [Display(Name = "Minimum Salary (RM)")]
        public decimal? MinimumSalary { get; set; }

        [Range(0, 999999, ErrorMessage = "Enter a valid salary amount.")]
        [Display(Name = "Maximum Salary (RM)")]
        public decimal? MaximumSalary { get; set; }

        [Range(1, 100, ErrorMessage = "Enter a value between 1 and 100.")]
        [Display(Name = "Number of Vacancies")]
        public int VacancyCount { get; set; } = 1;

        [Display(Name = "Application Deadline")]
        [DataType(DataType.Date)]
        public DateOnly? ApplicationDeadline { get; set; }

        [Required(ErrorMessage = "Job description is required.")]
        [StringLength(4000)]
        [Display(Name = "Job Description")]
        public string JobDescription { get; set; } = string.Empty;

        [StringLength(4000)]
        [Display(Name = "Responsibilities")]
        public string? Responsibilities { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (MinimumSalary.HasValue && MaximumSalary.HasValue && MaximumSalary < MinimumSalary)
            {
                yield return new ValidationResult(
                    "Maximum salary must be greater than or equal to minimum salary.",
                    new[] { nameof(MaximumSalary) });
            }

            if (ApplicationDeadline.HasValue && ApplicationDeadline.Value < DateOnly.FromDateTime(DateTime.Today))
            {
                yield return new ValidationResult(
                    "Application deadline cannot be in the past.",
                    new[] { nameof(ApplicationDeadline) });
            }
        }
    }

    /// <summary>
    /// Container for the Employer's Vacancies page: the new-vacancy form
    /// plus the employer's own posted jobs (loaded from the `jobs` table).
    /// </summary>
    public class VacanciesPageViewModel
    {
        public VacancyFormViewModel NewVacancy { get; set; } = new();
        public List<Job> PublishedVacancies { get; set; } = new();

        public string? SearchKeyword { get; set; }
        public string? FilterEmploymentType { get; set; }
        public string? FilterWorkplaceType { get; set; }
        public string? FilterLocation { get; set; }

        /// <summary>
        /// True when the Employer has saved a CompanyDetail profile, used to
        /// show a hint that Company Name/Location were auto-filled from it.
        /// </summary>
        public bool HasCompanyDetails { get; set; }
    }
}
