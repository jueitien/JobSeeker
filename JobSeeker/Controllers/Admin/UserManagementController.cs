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
    public class UserManagementController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public UserManagementController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context     = context;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index(
            string? keyword,
            string? role,
            string? status)
        {
            var query = _context.Users.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var term = keyword.Trim();
                query = query.Where(u =>
                    u.FullName.Contains(term) ||
                    (u.Email != null && u.Email.Contains(term)));
            }

            if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(u => u.AccountStatus == status);

            var users = await query
                .OrderBy(u => u.FullName)
                .ToListAsync();

            // Get roles for each user
            var items = new List<UserListItem>();
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                var userRole = roles.FirstOrDefault() ?? "Unknown";

                // Filter by role if specified
                if (!string.IsNullOrWhiteSpace(role) && userRole != role)
                    continue;

                items.Add(new UserListItem
                {
                    Id            = user.Id,
                    FullName      = user.FullName,
                    Email         = user.Email,
                    Role          = userRole,
                    AccountStatus = user.AccountStatus,
                    CreatedAt     = user.CreatedAt
                });
            }

            var viewModel = new UserManagementViewModel
            {
                Users          = items,
                SearchKeyword  = keyword,
                FilterRole     = role,
                FilterStatus   = status,
                TotalCount     = items.Count
            };

            return View("~/Views/Admin/UserManagement/Index.cshtml", viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Suspended(string? keyword, string? role)
        {
            var query = _context.Users
                .AsNoTracking()
                .Where(u => u.AccountStatus == "SUSPENDED");

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var term = keyword.Trim();
                query = query.Where(u =>
                    u.FullName.Contains(term) ||
                    (u.Email != null && u.Email.Contains(term)));
            }

            var users = await query.OrderBy(u => u.FullName).ToListAsync();

            var items = new List<UserListItem>();
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                var userRole = roles.FirstOrDefault() ?? "Unknown";

                if (!string.IsNullOrWhiteSpace(role) && userRole != role)
                    continue;

                items.Add(new UserListItem
                {
                    Id            = user.Id,
                    FullName      = user.FullName,
                    Email         = user.Email,
                    Role          = userRole,
                    AccountStatus = user.AccountStatus,
                    CreatedAt     = user.CreatedAt
                });
            }

            var viewModel = new UserManagementViewModel
            {
                Users         = items,
                SearchKeyword = keyword,
                FilterRole    = role,
                FilterStatus  = "SUSPENDED",
                TotalCount    = items.Count
            };

            return View("~/Views/Admin/UserManagement/Suspended.cshtml", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Suspend(string id, string? returnAction)
        {
            var admin = await _userManager.GetUserAsync(User);
            var user  = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            user.AccountStatus = "SUSPENDED";
            user.UpdatedAt     = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            await WriteAuditLog(
                admin?.Id,
                "USER_SUSPENDED",
                "User",
                description: $"Suspended user: {user.FullName} ({user.Email})");

            TempData["SuccessMessage"] = $"{user.FullName} has been suspended.";
            return RedirectToAction(returnAction == "Suspended" ? "Suspended" : "Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Activate(string id)
        {
            var admin = await _userManager.GetUserAsync(User);
            var user  = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            user.AccountStatus = "ACTIVE";
            user.UpdatedAt     = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            await WriteAuditLog(
                admin?.Id,
                "USER_ACTIVATED",
                "User",
                description: $"Activated user: {user.FullName} ({user.Email})");

            TempData["SuccessMessage"] = $"{user.FullName} has been reactivated.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deactivate(string id)
        {
            var admin = await _userManager.GetUserAsync(User);
            var user  = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            user.AccountStatus = "DEACTIVATED";
            user.UpdatedAt     = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            await WriteAuditLog(
                admin?.Id,
                "USER_DEACTIVATED",
                "User",
                description: $"Deactivated user: {user.FullName} ({user.Email})");

            TempData["SuccessMessage"] = $"{user.FullName} has been deactivated.";
            return RedirectToAction("Index");
        }

        // ─── Helper ────────────────────────────────────────────────────────────
        private async Task WriteAuditLog(
            string? userId,
            string actionType,
            string entityType,
            string? description = null)
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
