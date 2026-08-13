using JobSeeker.Data;
using JobSeeker.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JobSeeker.ViewComponents
{
    public class NotificationNavViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public NotificationNavViewComponent(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var userId = _userManager.GetUserId(UserClaimsPrincipal);
            var unreadCount = string.IsNullOrWhiteSpace(userId)
                ? 0
                : await _context.Notifications.CountAsync(notification =>
                    notification.UserId == userId && !notification.IsRead);

            return View(unreadCount);
        }
    }
}
