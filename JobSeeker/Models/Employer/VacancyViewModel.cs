using System.ComponentModel.DataAnnotations;
using JobSeeker.Models;

namespace JobSeeker.Models.Employer
{
    /// <summary>
    /// A single row in the "Skills Requirement" section of the Post a Vacancy
    /// form. Maps onto a JobRequiredSkill row (job_required_skills table).
    /// </summary>
    public class SkillRequirementViewModel
    {
        [Required(ErrorMessage = "Select a skill.")]
        [Display(Name = "Skill")]
        public long SkillId { get; set; }

        [Required(ErrorMessage = "Select a requirement type.")]
        [Display(Name = "Requirement Type")]
        public string RequirementType { get; set; } = "REQUIRED";   // REQUIRED, PREFERRED, NICE_TO_HAVE

        [Range(0.01, 100, ErrorMessage = "Weightage must be between 0.01 and 100.")]
        [Display(Name = "Importance Weightage (%)")]
        public decimal ImportanceWeight { get; set; }
    }

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

        /// <summary>
        /// Skills Requirement rows. Each row's ImportanceWeight must sum to
        /// exactly 100% across the whole list (enforced below).
        /// </summary>
        public List<SkillRequirementViewModel> SkillRequirements { get; set; } = new();

        /// <summary>
        /// Up to 3 images to attach to the vacancy posting (job_vacancy_images
        /// table). Stored in the dedicated "job-vacancies-images" S3 bucket.
        /// </summary>
        public List<Microsoft.AspNetCore.Http.IFormFile> VacancyImages { get; set; } = new();

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

            var validRows = SkillRequirements
                .Where(r => r.SkillId > 0)
                .ToList();

            if (validRows.Count == 0)
            {
                yield return new ValidationResult(
                    "Add at least one skill requirement.",
                    new[] { nameof(SkillRequirements) });
            }
            else
            {
                var duplicateSkillIds = validRows
                    .GroupBy(r => r.SkillId)
                    .Where(g => g.Count() > 1)
                    .Select(g => g.Key)
                    .ToList();

                if (duplicateSkillIds.Count > 0)
                {
                    yield return new ValidationResult(
                        "Each skill can only be added once.",
                        new[] { nameof(SkillRequirements) });
                }

                var totalWeight = validRows.Sum(r => r.ImportanceWeight);
                if (Math.Round(totalWeight, 2) != 100m)
                {
                    yield return new ValidationResult(
                        $"The total importance weightage for all skills must equal exactly 100%. Current total: {totalWeight:0.##}%.",
                        new[] { nameof(SkillRequirements) });
                }
            }

            var providedImages = VacancyImages.Where(f => f != null && f.Length > 0).ToList();

            if (providedImages.Count > 3)
            {
                yield return new ValidationResult(
                    "You can upload a maximum of 3 images.",
                    new[] { nameof(VacancyImages) });
            }

            const long maxImageBytes = 5 * 1024 * 1024;
            var allowedImageExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };

            foreach (var image in providedImages)
            {
                var extension = System.IO.Path.GetExtension(image.FileName).ToLowerInvariant();
                if (!allowedImageExtensions.Contains(extension))
                {
                    yield return new ValidationResult(
                        "Vacancy images must be JPG, JPEG, PNG, or WEBP.",
                        new[] { nameof(VacancyImages) });
                    break;
                }
            }

            foreach (var image in providedImages)
            {
                if (image.Length > maxImageBytes)
                {
                    yield return new ValidationResult(
                        "Each vacancy image must be 5 MB or smaller.",
                        new[] { nameof(VacancyImages) });
                    break;
                }
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

        /// <summary>All skills from the skills table, used to populate the skill dropdowns.</summary>
        public List<Skill> AvailableSkills { get; set; } = new();

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
