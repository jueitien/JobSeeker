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
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Dashboard()
        {
            var totalUsers = await _context.Users.CountAsync();

            var pendingEmployerVerifications = await _context.EmployerProfiles
                .CountAsync(e => e.VerificationStatus == "PENDING");

            var pendingJobApprovals = await _context.Jobs
                .CountAsync(j => j.ApprovalStatus == "PENDING");

            var suspendedAccounts = await _context.Users
                .CountAsync(u => u.AccountStatus == "SUSPENDED");

            var recentActivities = await _context.AuditLogs
                .AsNoTracking()
                .OrderByDescending(a => a.CreatedAt)
                .Take(10)
                .Select(a => new RecentActivityItem
                {
                    ActionType    = a.ActionType,
                    Description   = a.ActionDescription ?? a.ActionType,
                    PerformedBy   = a.User != null ? a.User.FullName : "System",
                    CreatedAt     = a.CreatedAt
                })
                .ToListAsync();

            var viewModel = new DashboardViewModel
            {
                TotalUsers                    = totalUsers,
                PendingEmployerVerifications  = pendingEmployerVerifications,
                PendingJobApprovals           = pendingJobApprovals,
                SuspendedAccounts             = suspendedAccounts,
                RecentActivities              = recentActivities
            };

            return View("~/Views/Admin/Dashboard.cshtml", viewModel);
        }
    }
}
