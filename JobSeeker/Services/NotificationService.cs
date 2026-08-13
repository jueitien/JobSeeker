using JobSeeker.Data;
using JobSeeker.Models;

namespace JobSeeker.Services
{
    public class NotificationService
    {
        private readonly ApplicationDbContext _context;

        public NotificationService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>Sends a notification to a specific user.</summary>
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
                UserId           = userId,
                NotificationType = notificationType,
                Title            = title,
                Message          = message,
                ReferenceType    = referenceType,
                ReferenceId      = referenceId,
                IsRead           = false,
                CreatedAt        = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
        }
    }
}
