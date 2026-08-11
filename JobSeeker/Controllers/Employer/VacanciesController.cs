using JobSeeker.Data;
using JobSeeker.Models;
using JobSeeker.Models.Employer;
using JobSeeker.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JobSeeker.Controllers.Employer
{
    [Authorize(Roles = "Employer")]
    public class VacanciesController : Controller
    {
        private const int MaxVacancyImages = 3;

        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly S3StorageService _s3Storage;
        private readonly ILogger<VacanciesController> _logger;

        public VacanciesController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            S3StorageService s3Storage,
            ILogger<VacanciesController> logger)
        {
            _context = context;
            _userManager = userManager;
            _s3Storage = s3Storage;
            _logger = logger;
        }

        // GET: /Vacancies
        public async Task<IActionResult> Index(
            string? searchKeyword,
            string? filterEmploymentType,
            string? filterWorkplaceType,
            string? filterLocation)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            IQueryable<Job> query = _context.Jobs
                .Where(j => j.EmployerId == user.Id);

            if (!string.IsNullOrWhiteSpace(searchKeyword))
            {
                var term = searchKeyword.Trim();
                query = query.Where(j =>
                    j.JobTitle.Contains(term) ||
                    j.CompanyName.Contains(term) ||
                    j.JobDescription.Contains(term));
            }

            if (!string.IsNullOrWhiteSpace(filterEmploymentType) && filterEmploymentType != "All")
            {
                query = query.Where(j => j.EmploymentType == filterEmploymentType);
            }

            if (!string.IsNullOrWhiteSpace(filterWorkplaceType) && filterWorkplaceType != "All")
            {
                query = query.Where(j => j.WorkplaceType == filterWorkplaceType);
            }

            if (!string.IsNullOrWhiteSpace(filterLocation))
            {
                query = query.Where(j => j.Location != null && j.Location.Contains(filterLocation));
            }

            var vacancies = await query
                .Include(j => j.VacancyImages)
                .OrderByDescending(j => j.CreatedAt)
                .ToListAsync();

            var company = await _context.EmployerProfiles
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.EmployerId == user.Id);

            var skills = await _context.Skills
                .AsNoTracking()
                .OrderBy(s => s.SkillName)
                .ToListAsync();

            var viewModel = new VacanciesPageViewModel
            {
                PublishedVacancies = vacancies,
                SearchKeyword = searchKeyword,
                FilterEmploymentType = filterEmploymentType,
                FilterWorkplaceType = filterWorkplaceType,
                FilterLocation = filterLocation,
                AvailableSkills = skills,
                NewVacancy = new VacancyFormViewModel
                {
                    // Auto-fill from the employer's saved company profile, if any.
                    // Location is capped at 200 chars to match the jobs table column width.
                    CompanyName = company?.CompanyName ?? string.Empty,
                    Location = company?.CompanyAddress is { Length: > 200 } addr ? addr[..200] : company?.CompanyAddress
                },
                HasCompanyDetails = company != null
            };

            return View("~/Views/Employer/Vacancies/Index.cshtml", viewModel);
        }

        // POST: /Vacancies/Post
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Post([Bind(Prefix = "NewVacancy")] VacancyFormViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            if (!ModelState.IsValid)
            {
                var vacancies = await _context.Jobs
                    .Where(j => j.EmployerId == user.Id)
                    .Include(j => j.VacancyImages)
                    .OrderByDescending(j => j.CreatedAt)
                    .ToListAsync();

                var hasCompany = await _context.EmployerProfiles
                    .AsNoTracking()
                    .AnyAsync(c => c.EmployerId == user.Id);

                var skills = await _context.Skills
                    .AsNoTracking()
                    .OrderBy(s => s.SkillName)
                    .ToListAsync();

                var viewModel = new VacanciesPageViewModel
                {
                    NewVacancy = model,
                    PublishedVacancies = vacancies,
                    HasCompanyDetails = hasCompany,
                    AvailableSkills = skills
                };
                return View("~/Views/Employer/Vacancies/Index.cshtml", viewModel);
            }

            var now = DateTime.UtcNow;

            var job = new Job
            {
                EmployerId = user.Id,
                CompanyName = model.CompanyName,
                JobTitle = model.JobTitle,
                JobDescription = model.JobDescription,
                Responsibilities = model.Responsibilities,
                MinimumQualification = model.MinimumQualification,
                PreferredFieldOfStudy = model.PreferredFieldOfStudy,
                MinimumExperienceYears = model.MinimumExperienceYears,
                EmploymentType = model.EmploymentType,
                WorkplaceType = model.WorkplaceType,
                Location = model.Location,
                MinimumSalary = model.MinimumSalary,
                MaximumSalary = model.MaximumSalary,
                VacancyCount = model.VacancyCount,
                ApplicationDeadline = model.ApplicationDeadline,
                ApprovalStatus = "PENDING",   // new postings must be approved by an Administrator
                JobStatus = "OPEN",
                IsReopenRequest = false,
                IsTestData = false,
                CreatedAt = now,
                UpdatedAt = now
            };

            _context.Jobs.Add(job);
            await _context.SaveChangesAsync();

            // Persist the Skills Requirement rows into job_required_skills.
            foreach (var skillRow in model.SkillRequirements.Where(r => r.SkillId > 0))
            {
                _context.JobRequiredSkills.Add(new JobRequiredSkill
                {
                    JobId = job.JobId,
                    SkillId = skillRow.SkillId,
                    RequirementType = skillRow.RequirementType,
                    ImportanceWeight = skillRow.ImportanceWeight
                });
            }

            await _context.SaveChangesAsync();

            // Upload up to 3 vacancy images into the "vacancy-images" folder
            // of the main S3 bucket and record them in job_vacancy_images.
            var imagesToUpload = model.VacancyImages
                .Where(f => f != null && f.Length > 0)
                .Take(MaxVacancyImages)
                .ToList();

            var imageUploadFailed = false;
            var displayOrder = 0;

            foreach (var image in imagesToUpload)
            {
                try
                {
                    var imageKey = await _s3Storage.UploadVacancyImageAsync(image, job.JobId.ToString());
                    _context.JobVacancyImages.Add(new JobVacancyImage
                    {
                        JobId = job.JobId,
                        ImageS3Key = imageKey,
                        DisplayOrder = displayOrder,
                        UploadedAt = now
                    });
                    displayOrder++;
                }
                catch (Exception ex)
                {
                    imageUploadFailed = true;
                    _logger.LogError(ex, "Failed to upload vacancy image for job {JobId}.", job.JobId);
                }
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = imageUploadFailed
                ? "Job vacancy submitted successfully. One or more vacancy images could not be uploaded."
                : "Job vacancy submitted successfully! It will be visible to job seekers once approved by an administrator.";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Vacancies/ViewImage/{id}
        // {id} is the JobVacancyImage id. Only the employer who owns the
        // vacancy may view its images through this action.
        [HttpGet]
        public async Task<IActionResult> ViewImage(long id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var image = await _context.JobVacancyImages
                .AsNoTracking()
                .Include(i => i.Job)
                .FirstOrDefaultAsync(i => i.JobVacancyImageId == id);

            if (image == null || image.Job.EmployerId != user.Id)
                return NotFound();

            try
            {
                var presignedUrl = await _s3Storage.GetVacancyImagePresignedUrlAsync(
                    image.ImageS3Key, TimeSpan.FromMinutes(5));
                return Redirect(presignedUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create presigned S3 URL for vacancy image {Key}.", image.ImageS3Key);
                TempData["ErrorMessage"] = "The vacancy image could not be opened.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: /Vacancies/Close/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Close(long id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var job = await _context.Jobs
                .FirstOrDefaultAsync(j => j.JobId == id && j.EmployerId == user.Id);

            if (job == null) return NotFound();

            job.JobStatus = "CLOSED";
            job.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"\"{job.JobTitle}\" has been closed.";
            return RedirectToAction(nameof(Index));
        }

        // POST: /Vacancies/Reopen/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reopen(long id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var job = await _context.Jobs
                .FirstOrDefaultAsync(j => j.JobId == id && j.EmployerId == user.Id);

            if (job == null) return NotFound();

            var wasRejected = job.ApprovalStatus == "REJECTED";

            job.JobStatus = "OPEN";
            // Re-submit for admin approval — a reopened rejected job must be reviewed again.
            if (wasRejected)
            {
                job.ApprovalStatus   = "PENDING";
                job.IsReopenRequest  = true;
                job.RejectionReason  = null;
            }
            job.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = wasRejected
                ? $"\"{job.JobTitle}\" has been resubmitted for admin approval."
                : $"\"{job.JobTitle}\" has been reopened.";
            return RedirectToAction(nameof(Index));
        }
    }
}
