using JobSeeker.Data;
using JobSeeker.Models;
using JobSeeker.Models.ViewModels.Admin;
using JobSeeker.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JobSeeker.Controllers.Admin
{
    [Authorize(Roles = "Administrator")]
    public class JobApprovalController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly S3StorageService _s3Storage;
        private readonly NotificationService _notifications;

        public JobApprovalController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            S3StorageService s3Storage,
            NotificationService notifications)
        {
            _context       = context;
            _userManager   = userManager;
            _s3Storage     = s3Storage;
            _notifications = notifications;
        }

        [HttpGet]
        public async Task<IActionResult> Pending(string? keyword)
            => await RenderList("PENDING", keyword, null, null,
                "~/Views/Admin/JobApproval/Pending.cshtml");

        [HttpGet]
        public async Task<IActionResult> Approved(
            string? keyword,
            string? employmentType,
            string? jobStatus)
            => await RenderList("APPROVED", keyword, employmentType, jobStatus,
                "~/Views/Admin/JobApproval/Approved.cshtml");

        [HttpGet]
        public async Task<IActionResult> Rejected(string? keyword)
            => await RenderList("REJECTED", keyword, null, null,
                "~/Views/Admin/JobApproval/Rejected.cshtml");

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(long id)
        {
            var admin = await _userManager.GetUserAsync(User);
            var job   = await _context.Jobs.FindAsync(id);
            if (job == null) return NotFound();

            job.ApprovalStatus = "APPROVED";
            job.ApprovedBy     = admin?.Id;
            job.ApprovedAt     = DateTime.UtcNow;
            job.JobStatus      = "OPEN";
            job.RejectionReason = null;
            job.UpdatedAt      = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            await WriteAuditLog(admin?.Id, "JOB_APPROVED", "Job",
                $"Approved job: {job.JobTitle} at {job.CompanyName}");

            if (job.EmployerId != null)
                await _notifications.SendAsync(
                    userId:           job.EmployerId,
                    notificationType: "JOB_APPROVED",
                    title:            "Your job posting has been approved",
                    message:          $"Your job posting \"{job.JobTitle}\" at {job.CompanyName} has been approved and is now live.",
                    referenceType:    "Job",
                    referenceId:      job.JobId);

            TempData["SuccessMessage"] = $"\"{job.JobTitle}\" has been approved.";
            return RedirectToAction("Pending");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(long id, string? reason)
        {
            var admin = await _userManager.GetUserAsync(User);
            var job   = await _context.Jobs.FindAsync(id);
            if (job == null) return NotFound();

            job.ApprovalStatus  = "REJECTED";
            job.RejectionReason = reason?.Trim();
            job.ApprovedBy      = admin?.Id;
            job.ApprovedAt      = DateTime.UtcNow;
            job.JobStatus       = "CLOSED";
            job.UpdatedAt       = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            await WriteAuditLog(admin?.Id, "JOB_REJECTED", "Job",
                $"Rejected job: {job.JobTitle} at {job.CompanyName}. Reason: {reason}");

            if (job.EmployerId != null)
                await _notifications.SendAsync(
                    userId:           job.EmployerId,
                    notificationType: "JOB_REJECTED",
                    title:            "Your job posting was rejected",
                    message:          string.IsNullOrWhiteSpace(reason)
                                        ? $"Your job posting \"{job.JobTitle}\" at {job.CompanyName} has been rejected. Please review and resubmit."
                                        : $"Your job posting \"{job.JobTitle}\" at {job.CompanyName} has been rejected. Reason: {reason.Trim()}",
                    referenceType:    "Job",
                    referenceId:      job.JobId);

            TempData["SuccessMessage"] = $"\"{job.JobTitle}\" has been rejected.";
            return RedirectToAction("Pending");
        }

        // ─── Shared list builder ───────────────────────────────────────────────
        private async Task<IActionResult> RenderList(
            string approvalStatus,
            string? keyword,
            string? employmentType,
            string? jobStatus,
            string viewPath)
        {
            var query = _context.Jobs
                .AsNoTracking()
                .Include(j => j.Approver)
                .Include(j => j.VacancyImages)
                .Where(j => j.ApprovalStatus == approvalStatus);

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var term = keyword.Trim();
                query = query.Where(j =>
                    j.JobTitle.Contains(term) ||
                    j.CompanyName.Contains(term));
            }

            if (!string.IsNullOrWhiteSpace(employmentType))
                query = query.Where(j => j.EmploymentType == employmentType);

            if (!string.IsNullOrWhiteSpace(jobStatus))
                query = query.Where(j => j.JobStatus == jobStatus);

            var jobs = await query
                .OrderByDescending(j => j.CreatedAt)
                .ToListAsync();

            var items = jobs.Select(j => new JobApprovalItem
            {
                JobId              = j.JobId,
                JobTitle           = j.JobTitle,
                CompanyName        = j.CompanyName,
                EmploymentType     = j.EmploymentType,
                WorkplaceType      = j.WorkplaceType,
                Location           = j.Location,
                MinimumSalary      = j.MinimumSalary,
                MaximumSalary      = j.MaximumSalary,
                ApplicationDeadline = j.ApplicationDeadline,
                ApprovalStatus     = j.ApprovalStatus,
                JobStatus          = j.JobStatus,
                RejectionReason    = j.RejectionReason,
                ApprovedByName     = j.Approver?.FullName,
                ApprovedAt         = j.ApprovedAt,
                CreatedAt          = j.CreatedAt,
                JobDescription     = j.JobDescription,
                Responsibilities   = j.Responsibilities,
                MinimumQualification     = j.MinimumQualification,
                PreferredFieldOfStudy    = j.PreferredFieldOfStudy,
                MinimumExperienceYears   = j.MinimumExperienceYears,
                VacancyCount             = j.VacancyCount,
                VacancyImageIds          = j.VacancyImages
                                            .OrderBy(i => i.DisplayOrder)
                                            .Select(i => i.JobVacancyImageId)
                                            .ToList(),
                IsReopenRequest          = j.IsReopenRequest
            }).ToList();

            var viewModel = new JobApprovalViewModel
            {
                Jobs                 = items,
                SearchKeyword        = keyword,
                FilterEmploymentType = employmentType,
                FilterJobStatus      = jobStatus,
                TotalCount           = items.Count
            };

            return View(viewPath, viewModel);
        }

        // GET: /JobApproval/VacancyImage/{imageId}
        // Returns a redirect to a 5-minute presigned S3 URL so admin can view vacancy images.
        [HttpGet]
        public async Task<IActionResult> VacancyImage(long imageId)
        {
            var image = await _context.JobVacancyImages
                .AsNoTracking()
                .FirstOrDefaultAsync(i => i.JobVacancyImageId == imageId);

            if (image == null) return NotFound();

            try
            {
                var url = await _s3Storage.GetVacancyImagePresignedUrlAsync(
                    image.ImageS3Key, TimeSpan.FromMinutes(5));
                return Redirect(url);
            }
            catch
            {
                return NotFound();
            }
        }

        private async Task WriteAuditLog(
            string? userId, string actionType,
            string entityType, string? description = null)
        {
            _context.AuditLogs.Add(new AuditLog
            {
                UserId            = userId,
                ActionType        = actionType,
                EntityType        = entityType,
                ActionDescription = description,
                IpAddress         = HttpContext.Connection.RemoteIpAddress?.ToString(),
                CreatedAt         = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();
        }
    }
}
