namespace JobSeeker.Models.ViewModels
{
    public class JobSeekerProfileDetailsViewModel
    {
        public JobSeekerProfile? Profile { get; set; }

        public IReadOnlyList<Resume> Resumes { get; set; } = Array.Empty<Resume>();

        public IReadOnlyList<Certification> Certifications { get; set; } = Array.Empty<Certification>();

        public IReadOnlyList<JobSeekerSkill> Skills { get; set; } = Array.Empty<JobSeekerSkill>();
    }
}