using Amazon.S3;
using Amazon.S3.Model;
using JobSeeker.Data;
using JobSeeker.Models;
using JobSeeker.Models.ViewModels.Admin;
using JobSeeker.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace JobSeeker.Controllers.Admin
{
    [Authorize(Roles = "Administrator")]
    public class ReportsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly S3StorageService _s3;
        private readonly IConfiguration _config;

        public ReportsController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            S3StorageService s3,
            IConfiguration config)
        {
            _context = context;
            _userManager = userManager;
            _s3 = s3;
            _config = config;
        }

        // ─── Bucket used for reports ──────────────────────────────────────────
        private string ReportsBucket =>
            _config["AWS:ReportsBucketName"]?.Trim()
            ?? _config["AWS:S3BucketName"]?.Trim()
            ?? string.Empty;

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var raw = await _context.SystemReports
                .AsNoTracking()
                .Include(r => r.Generator)
                .OrderByDescending(r => r.GeneratedAt)
                .ToListAsync();

            var reports = raw.Select(r => new ReportHistoryItem
            {
                SystemReportId  = r.SystemReportId,
                ReportName      = r.ReportName,
                ReportType      = r.ReportType,
                GeneratedByName = r.Generator.FullName,
                GeneratedAt     = r.GeneratedAt,
                HasFile         = !string.IsNullOrWhiteSpace(r.ReportS3Key),
                FileSizeBytes   = r.FileSizeBytes
            }).ToList();

            var viewModel = new ReportsViewModel { Reports = reports };
            return View("~/Views/Admin/Reports/Index.cshtml", viewModel);
        }

        // ─── Generate ─────────────────────────────────────────────────────────
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

            var effectiveTo   = (dateTo   ?? DateTime.UtcNow).Date.AddDays(1).AddTicks(-1);
            var effectiveFrom = (dateFrom ?? DateTime.UtcNow.AddYears(-1)).Date;

            // 1. Build CSV content from DB
            var (csvBytes, fileName) = await BuildCsvAsync(reportType, effectiveFrom, effectiveTo);

            // 2. Upload to S3
            string? s3Key = null;
            long?   fileSize = null;
            if (_s3.IsConfigured && csvBytes.Length > 0)
            {
                s3Key    = await UploadReportToS3Async(csvBytes, fileName, ReportsBucket);
                fileSize = csvBytes.Length;
            }

            // 3. Persist record
            var reportName = $"{HumanizeReportType(reportType)} – {DateTime.UtcNow:dd MMM yyyy HH:mm}";
            var parameters = System.Text.Json.JsonSerializer.Serialize(new
            {
                reportType,
                dateFrom = effectiveFrom.ToString("yyyy-MM-dd"),
                dateTo   = effectiveTo.ToString("yyyy-MM-dd")
            });

            var report = new SystemReport
            {
                GeneratedBy      = admin.Id,
                ReportName       = reportName,
                ReportType       = reportType,
                ReportParameters = parameters,
                OriginalFileName = fileName,
                ReportS3Key      = s3Key,
                FileContentType  = "text/csv",
                FileSizeBytes    = fileSize,
                GeneratedAt      = DateTime.UtcNow
            };

            _context.SystemReports.Add(report);
            await _context.SaveChangesAsync();

            await WriteAuditLog(admin.Id, "REPORT_GENERATED", "SystemReport",
                $"Generated report: {reportName}");

            TempData["SuccessMessage"] = $"Report \"{reportName}\" has been generated and saved to S3.";
            return RedirectToAction(nameof(Index));
        }

        // ─── Download (presigned URL) ─────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> Download(long id)
        {
            var report = await _context.SystemReports.FindAsync(id);
            if (report == null) return NotFound();
            if (string.IsNullOrWhiteSpace(report.ReportS3Key))
            {
                TempData["ErrorMessage"] = "No file is attached to this report.";
                return RedirectToAction(nameof(Index));
            }

            // Generate a 10-minute presigned download URL
            var url = await GetReportPresignedUrlAsync(report.ReportS3Key, ReportsBucket, TimeSpan.FromMinutes(10));
            return Redirect(url);
        }

        // ─── CSV builders ─────────────────────────────────────────────────────

        private async Task<(byte[] Bytes, string FileName)> BuildCsvAsync(
            string reportType, DateTime from, DateTime to)
        {
            var csv = reportType switch
            {
                "USER_SUMMARY"           => await BuildUserSummaryCsvAsync(from, to),
                "EMPLOYER_VERIFICATION"  => await BuildEmployerVerificationCsvAsync(from, to),
                "JOB_POSTINGS"           => await BuildJobPostingsCsvAsync(from, to),
                "APPLICATION_STATISTICS" => await BuildApplicationStatsCsvAsync(from, to),
                _                        => string.Empty
            };

            var slug      = reportType.ToLower().Replace('_', '-');
            var timestamp = DateTime.UtcNow.ToString("yyyyMMdd-HHmmss");
            var fileName  = $"report-{slug}-{timestamp}.csv";

            return (Encoding.UTF8.GetBytes(csv), fileName);
        }

        private async Task<string> BuildUserSummaryCsvAsync(DateTime from, DateTime to)
        {
            var users = await _userManager.Users
                .Where(u => u.CreatedAt >= from && u.CreatedAt <= to)
                .OrderBy(u => u.CreatedAt)
                .ToListAsync();

            var sb = new StringBuilder();
            sb.AppendLine("FullName,Email,Role,RegisteredAt,EmailConfirmed");

            foreach (var u in users)
            {
                var roles = await _userManager.GetRolesAsync(u);
                sb.AppendLine(CsvRow(
                    u.FullName,
                    u.Email ?? "",
                    string.Join("|", roles),
                    u.CreatedAt.ToString("yyyy-MM-dd HH:mm"),
                    u.EmailConfirmed.ToString()));
            }

            return sb.ToString();
        }

        private async Task<string> BuildEmployerVerificationCsvAsync(DateTime from, DateTime to)
        {
            var employers = await _context.EmployerProfiles
                .AsNoTracking()
                .Include(e => e.User)
                .Where(e => e.CreatedAt >= from && e.CreatedAt <= to)
                .OrderBy(e => e.CreatedAt)
                .ToListAsync();

            var sb = new StringBuilder();
            sb.AppendLine("CompanyName,Email,Industry,VerificationStatus,VerifiedAt,CreatedAt");

            foreach (var e in employers)
            {
                sb.AppendLine(CsvRow(
                    e.CompanyName,
                    e.User?.Email ?? "",
                    e.Industry ?? "",
                    e.VerificationStatus,
                    e.VerifiedAt?.ToString("yyyy-MM-dd HH:mm") ?? "",
                    e.CreatedAt.ToString("yyyy-MM-dd HH:mm")));
            }

            return sb.ToString();
        }

        private async Task<string> BuildJobPostingsCsvAsync(DateTime from, DateTime to)
        {
            var jobs = await _context.Jobs
                .AsNoTracking()
                .Include(j => j.Employer)
                .Where(j => j.CreatedAt >= from && j.CreatedAt <= to)
                .OrderBy(j => j.CreatedAt)
                .ToListAsync();

            var sb = new StringBuilder();
            sb.AppendLine("JobTitle,CompanyName,EmploymentType,Location,ApprovalStatus,JobStatus,Applications,PostedAt");

            foreach (var j in jobs)
            {
                var appCount = await _context.JobApplications.CountAsync(a => a.JobId == j.JobId);
                sb.AppendLine(CsvRow(
                    j.JobTitle,
                    j.CompanyName,
                    j.EmploymentType,
                    j.Location ?? "",
                    j.ApprovalStatus,
                    j.JobStatus,
                    appCount.ToString(),
                    j.CreatedAt.ToString("yyyy-MM-dd HH:mm")));
            }

            return sb.ToString();
        }

        private async Task<string> BuildApplicationStatsCsvAsync(DateTime from, DateTime to)
        {
            var apps = await _context.JobApplications
                .AsNoTracking()
                .Include(a => a.Job)
                .Include(a => a.JobSeekerProfile).ThenInclude(p => p!.User)
                .Where(a => a.AppliedAt >= from && a.AppliedAt <= to)
                .OrderBy(a => a.AppliedAt)
                .ToListAsync();

            var sb = new StringBuilder();
            sb.AppendLine("ApplicantName,ApplicantEmail,JobTitle,CompanyName,Status,MatchScore,AppliedAt");

            foreach (var a in apps)
            {
                sb.AppendLine(CsvRow(
                    a.JobSeekerProfile?.User?.FullName ?? "",
                    a.JobSeekerProfile?.User?.Email    ?? "",
                    a.Job?.JobTitle                    ?? "",
                    a.Job?.CompanyName                 ?? "",
                    a.ApplicationStatus,
                    a.MatchPercentageAtApplication?.ToString("F1") ?? "",
                    a.AppliedAt.ToString("yyyy-MM-dd HH:mm")));
            }

            return sb.ToString();
        }

        // ─── S3 helpers ───────────────────────────────────────────────────────

        private async Task<string> UploadReportToS3Async(
            byte[] bytes, string fileName, string bucketName)
        {
            var s3Client = HttpContext.RequestServices.GetRequiredService<IAmazonS3>();
            var key      = $"reports/{DateTime.UtcNow:yyyy/MM}/{Guid.NewGuid():N}-{fileName}";

            using var stream = new MemoryStream(bytes);
            var request = new PutObjectRequest
            {
                BucketName  = bucketName,
                Key         = key,
                InputStream = stream,
                ContentType = "text/csv"
            };

            await s3Client.PutObjectAsync(request);
            return key;
        }

        private async Task<string> GetReportPresignedUrlAsync(
            string key, string bucketName, TimeSpan expiresIn)
        {
            var s3Client = HttpContext.RequestServices.GetRequiredService<IAmazonS3>();
            var request  = new GetPreSignedUrlRequest
            {
                BucketName  = bucketName,
                Key         = key,
                Verb        = HttpVerb.GET,
                Expires     = DateTime.UtcNow.Add(expiresIn),
                ResponseHeaderOverrides =
                {
                    // Tell the browser to download instead of opening in-tab
                    ContentDisposition = $"attachment; filename=\"{Path.GetFileName(key)}\""
                }
            };

            return await s3Client.GetPreSignedURLAsync(request);
        }

        // ─── Utilities ────────────────────────────────────────────────────────

        /// <summary>Escapes a single CSV field (wraps in quotes if needed).</summary>
        private static string EscapeCsvField(string? value)
        {
            if (string.IsNullOrEmpty(value)) return string.Empty;
            if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
                return $"\"{value.Replace("\"", "\"\"")}\"";
            return value;
        }

        private static string CsvRow(params string[] fields) =>
            string.Join(",", fields.Select(EscapeCsvField));

        private static string HumanizeReportType(string t) => t switch
        {
            "USER_SUMMARY"           => "User Summary Report",
            "EMPLOYER_VERIFICATION"  => "Employer Verification Report",
            "JOB_POSTINGS"           => "Job Postings Report",
            "APPLICATION_STATISTICS" => "Application Statistics Report",
            _                        => t
        };

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
