namespace JobSeeker.Models.Employer
{
    /// <summary>
    /// Page view model for the Employer's "Applicants" page, backed by the
    /// job_applications table (JobApplication entity) joined with Job and
    /// JobSeeker information for display + filtering.
    /// </summary>
    public class ApplicantsPageViewModel
    {
        public List<JobApplication> Applications { get; set; } = new();

        /// <summary>Employer's own jobs, used to populate the "Job" filter dropdown.</summary>
        public List<Job> EmployerJobs { get; set; } = new();

        // Filter / search state (round-tripped back into the form)
        public string? SearchKeyword { get; set; }
        public long? FilterJobId { get; set; }
        public string? FilterStatus { get; set; }
        public string? SortBy { get; set; }
    }
}
