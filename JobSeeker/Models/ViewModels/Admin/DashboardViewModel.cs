namespace JobSeeker.Models.ViewModels.Admin
{
    public class DashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int PendingEmployerVerifications { get; set; }
        public int PendingJobApprovals { get; set; }
        public int SuspendedAccounts { get; set; }
        public List<RecentActivityItem> RecentActivities { get; set; } = new();
    }

    public class RecentActivityItem
    {
        public string ActionType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? PerformedBy { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
