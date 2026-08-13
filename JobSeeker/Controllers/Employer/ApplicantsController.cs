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
    public class ApplicantsController : Controller
    {
        private static readonly string[] ValidStatuses =
        {
            "SUBMITTED", "UNDER_REVIEW", "SHORTLISTED", "INTERVIEW", "OFFERED", "HIRED", "REJECTED", "WITHDRAWN"
        };

        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly S3StorageService _s3Storage;
        private readonly ILogger<ApplicantsController> _logger;

        public ApplicantsController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            S3StorageService s3Storage,
            ILogger<ApplicantsController> logger)
        {
            _context = context;
            _userManager = userManager;
            _s3Storage = s3Storage;
            _logger = logger;
        }

        /// <summary>Helper method to create a notification for an applicant status change.</summary>
        private async Task CreateNotificationAsync(string userId, string title, string message, string referenceType, long referenceId)
        {
            var notification = new Notification
            {
                UserId = userId,
                NotificationType = "APPLICANT_STATUS_CHANGE",
                Title = title,
                Message = message,
                ReferenceType = referenceType,
                ReferenceId = referenceId,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };
            _context.Notifications.Add(notification);
        }

        // GET: /Applicants
        [HttpGet]
        public async Task<IActionResult> Index(
            string? searchKeyword,
            long? filterJobId,
            string? filterStatus,
            string? sortBy)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            // Applicants can only be viewed for jobs owned by the current employer.
            var employerJobIds = await _context.Jobs
                .Where(j => j.EmployerId == user.Id)
                .Select(j => j.JobId)
                .ToListAsync();

            IQueryable<JobApplication> query = _context.JobApplications
                .AsNoTracking()
                .Include(a => a.Job)
                .Include(a => a.Resume)
                .Include(a => a.JobSeekerProfile)
                    .ThenInclude(p => p.User)
                .Where(a => employerJobIds.Contains(a.JobId));

            if (filterJobId.HasValue)
            {
                query = query.Where(a => a.JobId == filterJobId.Value);
            }

            if (!string.IsNullOrWhiteSpace(filterStatus) && filterStatus != "All")
            {
                query = query.Where(a => a.ApplicationStatus == filterStatus);
            }

            if (!string.IsNullOrWhiteSpace(searchKeyword))
            {
                var term = searchKeyword.Trim();
                query = query.Where(a =>
                    a.JobSeekerProfile.User.FullName.Contains(term) ||
                    a.Job.JobTitle.Contains(term) ||
                    (a.Resume.ResumeTitle != null && a.Resume.ResumeTitle.Contains(term)));
            }

            query = sortBy switch
            {
                "oldest" => query.OrderBy(a => a.AppliedAt),
                "match" => query.OrderByDescending(a => a.MatchPercentageAtApplication ?? 0),
                "name" => query.OrderBy(a => a.JobSeekerProfile.User.FullName),
                _ => query.OrderByDescending(a => a.AppliedAt) // "newest" (default)
            };

            var applications = await query.ToListAsync();

            var employerJobs = await _context.Jobs
                .AsNoTracking()
                .Where(j => j.EmployerId == user.Id)
                .OrderByDescending(j => j.CreatedAt)
                .ToListAsync();

            var viewModel = new ApplicantsPageViewModel
            {
                Applications = applications,
                EmployerJobs = employerJobs,
                SearchKeyword = searchKeyword,
                FilterJobId = filterJobId,
                FilterStatus = filterStatus,
                SortBy = sortBy
            };

            return View("~/Views/Employer/Applicants/Index.cshtml", viewModel);
        }

        // GET: /Applicants/ViewResume/{id}
        // {id} is the JobApplication id. Only the employer who owns the job
        // that the application was submitted to may view the attached resume.
        [HttpGet]
        public async Task<IActionResult> ViewResume(long id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var application = await _context.JobApplications
                .AsNoTracking()
                .Include(a => a.Job)
                .Include(a => a.Resume)
                .FirstOrDefaultAsync(a => a.ApplicationId == id);

            if (application == null
                || application.Job.EmployerId != user.Id
                || application.Resume == null
                || string.IsNullOrWhiteSpace(application.Resume.ResumeS3Key))
            {
                return NotFound();
            }

            try
            {
                var presignedUrl = await _s3Storage.GetPresignedUrlAsync(
                    application.Resume.ResumeS3Key, TimeSpan.FromMinutes(5));
                return Redirect(presignedUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create presigned S3 URL for resume {Key}.", application.Resume.ResumeS3Key);
                TempData["ErrorMessage"] = "The resume could not be opened.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: /Applicants/UpdateStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(long id, string status, string? employerNotes)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            if (!ValidStatuses.Contains(status))
            {
                TempData["ErrorMessage"] = "Invalid application status.";
                return RedirectToAction(nameof(Index));
            }

            var application = await _context.JobApplications
                .Include(a => a.Job)
                .FirstOrDefaultAsync(a => a.ApplicationId == id);

            if (application == null || application.Job.EmployerId != user.Id)
            {
                return NotFound();
            }

            application.ApplicationStatus = status;
            application.EmployerNotes = string.IsNullOrWhiteSpace(employerNotes) ? null : employerNotes.Trim();
            application.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Create notifications for applicant status change
            var statusDisplayName = status switch
            {
                "SUBMITTED" => "Submitted",
                "UNDER_REVIEW" => "Under Review",
                "SHORTLISTED" => "Shortlisted",
                "INTERVIEW" => "Interview",
                "OFFERED" => "Offered",
                "HIRED" => "Hired",
                "REJECTED" => "Rejected",
                "WITHDRAWN" => "Withdrawn",
                _ => status
            };

            // Notification for job seeker
            await CreateNotificationAsync(
                application.JobSeekerId,
                "Application Status Updated",
                $"Your application for \"{application.Job.JobTitle}\" is now {statusDisplayName}.",
                "Application",
                application.ApplicationId);

            // Notification for employer
            await CreateNotificationAsync(
                user.Id,
                "Applicant Status Updated",
                $"Applicant status for \"{application.Job.JobTitle}\" has been updated to {statusDisplayName}.",
                "Application",
                application.ApplicationId);

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Applicant status updated.";
            return RedirectToAction(nameof(Index));
        }
    }
}
