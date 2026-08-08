using JobSeeker.Data;
using JobSeeker.Models;
using JobSeeker.Models.ViewModels.Admin;
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

        public JobApprovalController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
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
                CreatedAt          = j.CreatedAt
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
