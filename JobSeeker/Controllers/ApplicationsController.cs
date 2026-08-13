using JobSeeker.Data;
using JobSeeker.Models;
using JobSeeker.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JobSeeker.Controllers
{
    [Authorize(Roles = UserRoles.JobSeeker)]
    public class ApplicationsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly S3StorageService _s3Storage;
        private readonly ILogger<ApplicationsController> _logger;

        public ApplicationsController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            S3StorageService s3Storage,
            ILogger<ApplicationsController> logger)
        {
            _context = context;
            _userManager = userManager;
            _s3Storage = s3Storage;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? status)
        {
            var user = await GetCurrentUserAsync();

            var query = _context.JobApplications
                .AsNoTracking()
                .Include(x => x.Job)
                    .ThenInclude(x => x.VacancyImages)
                .Include(x => x.Resume)
                .Where(x => x.JobSeekerId == user.Id);

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(x => x.ApplicationStatus == status);
            }

            var applications = await query
                .OrderByDescending(x => x.AppliedAt)
                .ToListAsync();

            ViewBag.SelectedStatus = status;
            return View(applications);
        }


        // Lets the Job Seeker view an image for a vacancy they have applied to.
        [HttpGet]
        public async Task<IActionResult> VacancyImage(long id)
        {
            var user = await GetCurrentUserAsync();

            var image = await _context.JobVacancyImages
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.JobVacancyImageId == id);

            if (image == null)
                return NotFound();

            var ownsApplication = await _context.JobApplications
                .AsNoTracking()
                .AnyAsync(x => x.JobSeekerId == user.Id && x.JobId == image.JobId);

            if (!ownsApplication)
                return NotFound();

            try
            {
                var url = await _s3Storage.GetVacancyImagePresignedUrlAsync(
                    image.ImageS3Key, TimeSpan.FromMinutes(10));

                return Redirect(url);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Could not load application vacancy image {VacancyImageId}.", id);
                return NotFound();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Apply(long jobId)
        {
            var user = await GetCurrentUserAsync();

            if (await _context.JobApplications.AnyAsync(x =>
                    x.JobId == jobId && x.JobSeekerId == user.Id))
            {
                TempData["InfoMessage"] = "You have already applied for this job.";
                return RedirectToAction("JobSeeker", "Home");
            }

            var profile = await _context.JobSeekerProfiles
                .FirstOrDefaultAsync(x => x.JobSeekerId == user.Id);

            if (profile == null)
            {
                TempData["ErrorMessage"] = "Create your profile before applying for jobs.";
                return RedirectToAction("Edit", "Profile");
            }

            var resume = await _context.Resumes
                .Where(x => x.JobSeekerId == user.Id)
                .OrderByDescending(x => x.IsPrimary)
                .ThenByDescending(x => x.UploadedAt)
                .FirstOrDefaultAsync();

            if (resume == null)
            {
                TempData["ErrorMessage"] = "Upload a resume in Edit Profile before applying for jobs.";
                return RedirectToAction("Edit", "Profile");
            }

            var job = await _context.Jobs
                .Include(x => x.RequiredSkills)
                    .ThenInclude(x => x.Skill)
                .FirstOrDefaultAsync(x =>
                    x.JobId == jobId &&
                    x.ApprovalStatus == "APPROVED" &&
                    x.JobStatus == "OPEN");

            if (job == null)
            {
                TempData["ErrorMessage"] = "This job is no longer available.";
                return RedirectToAction("JobSeeker", "Home");
            }

            var seekerSkillIds = (await _context.JobSeekerSkills
                    .Where(x => x.JobSeekerId == user.Id)
                    .Select(x => x.SkillId)
                    .ToListAsync())
                .ToHashSet();

            var match = JobMatchCalculator.Calculate(profile, seekerSkillIds, job);
            var now = DateTime.UtcNow;

            var application = new JobApplication
            {
                JobId = job.JobId,
                JobSeekerId = user.Id,
                ResumeId = resume.ResumeId,
                MatchPercentageAtApplication = match.Percentage,
                ApplicationStatus = "SUBMITTED",
                AppliedAt = now,
                UpdatedAt = now
            };

            await using var transaction = await _context.Database.BeginTransactionAsync();

            _context.JobApplications.Add(application);

            // Save once to obtain the SQL Server identity value used by the
            // notification's generic reference fields.
            await _context.SaveChangesAsync();

            _context.Notifications.Add(new Notification
            {
                UserId = user.Id,
                NotificationType = "APPLICATION_SUBMITTED",
                Title = "Application submitted",
                Message = $"Your application for {job.JobTitle} at {job.CompanyName} was submitted on {now:dd MMM yyyy 'at' HH:mm} UTC.",
                ReferenceType = "JOB_APPLICATION",
                ReferenceId = application.ApplicationId,
                IsRead = false,
                CreatedAt = now
            });

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            TempData["SuccessMessage"] = $"Application submitted to {job.CompanyName}.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Withdraw(long id)
        {
            var user = await GetCurrentUserAsync();
            var application = await _context.JobApplications
                .FirstOrDefaultAsync(x => x.ApplicationId == id && x.JobSeekerId == user.Id);

            if (application == null)
                return NotFound();

            if (application.ApplicationStatus is "HIRED" or "REJECTED")
            {
                TempData["ErrorMessage"] = "This application can no longer be withdrawn.";
                return RedirectToAction(nameof(Index));
            }

            application.ApplicationStatus = "WITHDRAWN";
            application.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Application withdrawn.";
            return RedirectToAction(nameof(Index));
        }

        private async Task<ApplicationUser> GetCurrentUserAsync()
        {
            return await _userManager.GetUserAsync(User)
                ?? throw new InvalidOperationException("The current user could not be loaded.");
        }
    }
}
