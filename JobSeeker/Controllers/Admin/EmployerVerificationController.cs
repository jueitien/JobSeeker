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
    public class EmployerVerificationController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public EmployerVerificationController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Pending(string? keyword)
            => await RenderList("PENDING", keyword, null,
                "~/Views/Admin/EmployerVerification/Pending.cshtml");

        [HttpGet]
        public async Task<IActionResult> Approved(string? keyword, string? industry)
            => await RenderList("APPROVED", keyword, industry,
                "~/Views/Admin/EmployerVerification/Approved.cshtml");

        [HttpGet]
        public async Task<IActionResult> Rejected(string? keyword)
            => await RenderList("REJECTED", keyword, null,
                "~/Views/Admin/EmployerVerification/Rejected.cshtml");

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(string id)
        {
            var admin    = await _userManager.GetUserAsync(User);
            var employer = await _context.EmployerProfiles.FindAsync(id);
            if (employer == null) return NotFound();

            employer.VerificationStatus  = "APPROVED";
            employer.VerificationRemarks = null;
            employer.VerifiedBy          = admin?.Id;
            employer.VerifiedAt          = DateTime.UtcNow;
            employer.UpdatedAt           = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            await WriteAuditLog(admin?.Id, "EMPLOYER_APPROVED", "EmployerProfile",
                $"Approved employer: {employer.CompanyName}");

            TempData["SuccessMessage"] = $"{employer.CompanyName} has been approved.";
            return RedirectToAction("Pending");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(string id, string? remarks)
        {
            var admin    = await _userManager.GetUserAsync(User);
            var employer = await _context.EmployerProfiles.FindAsync(id);
            if (employer == null) return NotFound();

            employer.VerificationStatus  = "REJECTED";
            employer.VerificationRemarks = remarks?.Trim();
            employer.VerifiedBy          = admin?.Id;
            employer.VerifiedAt          = DateTime.UtcNow;
            employer.UpdatedAt           = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            await WriteAuditLog(admin?.Id, "EMPLOYER_REJECTED", "EmployerProfile",
                $"Rejected employer: {employer.CompanyName}. Reason: {remarks}");

            TempData["SuccessMessage"] = $"{employer.CompanyName} has been rejected.";
            return RedirectToAction("Pending");
        }

        // ─── Shared list builder ───────────────────────────────────────────────
        private async Task<IActionResult> RenderList(
            string status,
            string? keyword,
            string? industry,
            string viewPath)
        {
            var query = _context.EmployerProfiles
                .AsNoTracking()
                .Include(e => e.User)
                .Include(e => e.Verifier)
                .Where(e => e.VerificationStatus == status);

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var term = keyword.Trim();
                query = query.Where(e =>
                    e.CompanyName.Contains(term) ||
                    e.User.Email!.Contains(term));
            }

            if (!string.IsNullOrWhiteSpace(industry))
                query = query.Where(e => e.Industry == industry);

            var employers = await query
                .OrderByDescending(e => e.CreatedAt)
                .ToListAsync();

            var items = employers.Select(e => new EmployerVerificationItem
            {
                EmployerId               = e.EmployerId,
                CompanyName              = e.CompanyName,
                Email                    = e.User.Email,
                Industry                 = e.Industry,
                CompanySize              = e.CompanySize,
                CompanyRegistrationNumber = e.CompanyRegistrationNumber,
                VerificationStatus       = e.VerificationStatus,
                VerificationRemarks      = e.VerificationRemarks,
                VerifiedByName           = e.Verifier?.FullName,
                VerifiedAt               = e.VerifiedAt,
                CreatedAt                = e.CreatedAt
            }).ToList();

            var viewModel = new EmployerVerificationViewModel
            {
                Employers      = items,
                SearchKeyword  = keyword,
                FilterIndustry = industry,
                TotalCount     = items.Count
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
