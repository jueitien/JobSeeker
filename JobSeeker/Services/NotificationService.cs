using JobSeeker.Data;
using JobSeeker.Models;

namespace JobSeeker.Services
{
    /// <summary>
    /// Persists the original Task #1 in-app notification in RDS.
    /// Task #2 external administrator alerts are intentionally triggered only
    /// from the employer vacancy workflow, so normal user notifications are
    /// never broadcast to the administrator SNS topic.
    /// </summary>
    public class NotificationService
    {
        private readonly ApplicationDbContext _context;

        public NotificationService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task SendAsync(
            string userId,
            string notificationType,
            string title,
            string message,
            string? referenceType = null,
            long? referenceId = null)
        {
            _context.Notifications.Add(new Notification
            {
                UserId = userId,
                NotificationType = notificationType,
                Title = title,
                Message = message,
                ReferenceType = referenceType,
                ReferenceId = referenceId,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
        }
    }
}
