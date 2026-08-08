using JobSeeker.Data;
using JobSeeker.Models;
using JobSeeker.Models.Employer;
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

        public ApplicantsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
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

            TempData["SuccessMessage"] = "Applicant status updated.";
            return RedirectToAction(nameof(Index));
        }
    }
}
