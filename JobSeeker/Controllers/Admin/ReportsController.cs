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
    public class ReportsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ReportsController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var reports = await _context.SystemReports
                .AsNoTracking()
                .Include(r => r.Generator)
                .OrderByDescending(r => r.GeneratedAt)
                .Select(r => new ReportHistoryItem
                {
                    SystemReportId  = r.SystemReportId,
                    ReportName      = r.ReportName,
                    ReportType      = r.ReportType,
                    GeneratedByName = r.Generator.FullName,
                    GeneratedAt     = r.GeneratedAt
                })
                .ToListAsync();

            var viewModel = new ReportsViewModel { Reports = reports };
            return View("~/Views/Admin/Reports/Index.cshtml", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Generate(
            string reportType,
            DateTime? dateFrom,
            DateTime? dateTo)
        {
            var admin = await _userManager.GetUserAsync(User);
            if (admin == null) return Challenge();

            if (string.IsNullOrWhiteSpace(reportType))
            {
                TempData["ErrorMessage"] = "Please select a report type.";
                return RedirectToAction(nameof(Index));
            }

            // Build summary data based on report type
            var reportName = $"{reportType} – {DateTime.UtcNow:MMM yyyy}";
            var parameters = System.Text.Json.JsonSerializer.Serialize(new
            {
                reportType,
                dateFrom = dateFrom?.ToString("yyyy-MM-dd"),
                dateTo   = dateTo?.ToString("yyyy-MM-dd")
            });

            var report = new SystemReport
            {
                GeneratedBy      = admin.Id,
                ReportName       = reportName,
                ReportType       = reportType,
                ReportParameters = parameters,
                GeneratedAt      = DateTime.UtcNow
            };

            _context.SystemReports.Add(report);
            await _context.SaveChangesAsync();

            await WriteAuditLog(admin.Id, "REPORT_GENERATED", "SystemReport",
                $"Generated report: {reportName}");

            TempData["SuccessMessage"] = $"Report \"{reportName}\" has been generated.";
            return RedirectToAction(nameof(Index));
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
