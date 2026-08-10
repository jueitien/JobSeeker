namespace JobSeeker.Models.ViewModels.Admin
{
    public class ReportsViewModel
    {
        public List<ReportHistoryItem> Reports { get; set; } = new();

        // Form fields
        public string? ReportType { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
    }

    public class ReportHistoryItem
    {
        public long SystemReportId { get; set; }
        public string ReportName { get; set; } = string.Empty;
        public string ReportType { get; set; } = string.Empty;
        public string GeneratedByName { get; set; } = string.Empty;
        public DateTime GeneratedAt { get; set; }
        public bool HasFile { get; set; }
        public long? FileSizeBytes { get; set; }
    }
}
