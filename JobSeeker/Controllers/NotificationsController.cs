using JobSeeker.Data;
using JobSeeker.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JobSeeker.Controllers
{
    [Authorize(Roles = $"{UserRoles.JobSeeker},{UserRoles.Employer}")]
    public class NotificationsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public NotificationsController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? filter)
        {
            var userId = GetCurrentUserId();
            var selectedFilter = NormalizeFilter(filter);

            var userNotifications = _context.Notifications
                .AsNoTracking()
                .Where(notification => notification.UserId == userId);

            ViewBag.UnreadCount = await userNotifications
                .CountAsync(notification => !notification.IsRead);
            ViewBag.SelectedFilter = selectedFilter;

            if (selectedFilter == "unread")
            {
                userNotifications = userNotifications.Where(notification => !notification.IsRead);
            }
            else if (selectedFilter == "read")
            {
                userNotifications = userNotifications.Where(notification => notification.IsRead);
            }

            var notifications = await userNotifications
                .OrderByDescending(notification => notification.CreatedAt)
                .ToListAsync();

            return View(notifications);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsRead(long id, string? filter)
        {
            var userId = GetCurrentUserId();
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(item =>
                    item.NotificationId == id && item.UserId == userId);

            if (notification == null)
            {
                return NotFound();
            }

            if (!notification.IsRead)
            {
                notification.IsRead = true;
                notification.ReadAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index), new { filter = NormalizeFilter(filter) });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var userId = GetCurrentUserId();
            var unreadNotifications = await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync();

            if (unreadNotifications.Any())
            {
                var now = DateTime.UtcNow;
                foreach (var notification in unreadNotifications)
                {
                    notification.IsRead = true;
                    notification.ReadAt = now;
                }
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private string GetCurrentUserId()
        {
            return _userManager.GetUserId(User)
                ?? throw new InvalidOperationException("The current user could not be loaded.");
        }

        private static string? NormalizeFilter(string? filter)
        {
            var normalized = filter?.Trim().ToLowerInvariant();
            return normalized is "unread" or "read" ? normalized : null;
        }
    }
}
