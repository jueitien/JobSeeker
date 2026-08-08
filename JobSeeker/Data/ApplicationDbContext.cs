using JobSeeker.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace JobSeeker.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public DbSet<JobSeekerProfile> JobSeekerProfiles => Set<JobSeekerProfile>();
        public DbSet<Resume> Resumes => Set<Resume>();
        public DbSet<Certification> Certifications => Set<Certification>();
        public DbSet<Skill> Skills => Set<Skill>();
        public DbSet<JobSeekerSkill> JobSeekerSkills => Set<JobSeekerSkill>();
        public DbSet<Job> Jobs => Set<Job>();
        public DbSet<JobRequiredSkill> JobRequiredSkills => Set<JobRequiredSkill>();
        public DbSet<JobApplication> JobApplications => Set<JobApplication>();
        public DbSet<EmployerProfile> EmployerProfiles => Set<EmployerProfile>();
        public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
        public DbSet<SystemReport> SystemReports => Set<SystemReport>();

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<JobSeekerProfile>(entity =>
            {
                entity.ToTable("job_seeker_profiles");
                entity.HasKey(profile => profile.JobSeekerId);
                entity.Property(profile => profile.ProfileDescription).HasColumnType("nvarchar(max)");
                entity.Property(profile => profile.CareerObjective).HasColumnType("nvarchar(max)");
                entity.Property(profile => profile.HighestQualification).HasMaxLength(150);
                entity.Property(profile => profile.FieldOfStudy).HasMaxLength(150);
                entity.Property(profile => profile.UniversityName).HasMaxLength(200);
                entity.Property(profile => profile.GraduationYear).HasColumnType("int");
                entity.Property(profile => profile.ExperienceDescription).HasColumnType("nvarchar(max)");
                entity.Property(profile => profile.PreferredJobTitle).HasMaxLength(200);
                entity.Property(profile => profile.PreferredLocation).HasMaxLength(150);
                entity.Property(profile => profile.ExpectedSalary).HasPrecision(12, 2);
                entity.Property(profile => profile.AvailabilityDate).HasColumnType("date");
                entity.Property(profile => profile.JobSeekerId).HasMaxLength(450);
                entity.Property(profile => profile.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(profile => profile.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.HasOne(profile => profile.User)
                    .WithOne(user => user.JobSeekerProfile)
                    .HasForeignKey<JobSeekerProfile>(profile => profile.JobSeekerId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(profile => profile.Resumes)
                    .WithOne(resume => resume.JobSeekerProfile)
                    .HasForeignKey(resume => resume.JobSeekerId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(profile => profile.Certifications)
                    .WithOne(certification => certification.JobSeekerProfile)
                    .HasForeignKey(certification => certification.JobSeekerId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(profile => profile.JobSeekerSkills)
                    .WithOne(jobSeekerSkill => jobSeekerSkill.JobSeekerProfile)
                    .HasForeignKey(jobSeekerSkill => jobSeekerSkill.JobSeekerId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(profile => profile.JobApplications)
                    .WithOne(application => application.JobSeekerProfile)
                    .HasForeignKey(application => application.JobSeekerId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Resume>(entity =>
            {
                entity.ToTable("resumes");
                entity.HasKey(resume => resume.ResumeId);
                entity.Property(resume => resume.JobSeekerId).HasMaxLength(450);
                entity.Property(resume => resume.ResumeTitle).HasMaxLength(200);
                entity.Property(resume => resume.ResumeDescription).HasColumnType("nvarchar(max)");
                entity.Property(resume => resume.ResumeS3Key).HasMaxLength(1024).IsRequired();
                entity.Property(resume => resume.ExtractedText).HasColumnType("nvarchar(max)");
                entity.Property(resume => resume.UploadedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            });

            modelBuilder.Entity<Certification>(entity =>
            {
                entity.ToTable("certifications");
                entity.HasKey(certification => certification.CertificationId);
                entity.Property(certification => certification.JobSeekerId).HasMaxLength(450);
                entity.Property(certification => certification.CertificationName).HasMaxLength(200).IsRequired();
                entity.Property(certification => certification.Description).HasColumnType("nvarchar(max)");
                entity.Property(certification => certification.CertificateS3Key).HasMaxLength(1024).IsRequired();
                entity.Property(certification => certification.UploadedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            });

            modelBuilder.Entity<Skill>(entity =>
            {
                entity.ToTable("skills");
                entity.HasKey(skill => skill.SkillId);
                entity.Property(skill => skill.SkillName).HasMaxLength(150).IsRequired();
                entity.Property(skill => skill.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.HasIndex(skill => skill.SkillName).IsUnique();
            });

            modelBuilder.Entity<JobSeekerSkill>(entity =>
            {
                entity.ToTable("job_seeker_skills");
                entity.HasKey(jobSeekerSkill => jobSeekerSkill.JobSeekerSkillId);
                entity.Property(jobSeekerSkill => jobSeekerSkill.JobSeekerId).HasMaxLength(450);
                entity.Property(jobSeekerSkill => jobSeekerSkill.ProficiencyLevel).HasMaxLength(30);
                entity.Property(jobSeekerSkill => jobSeekerSkill.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.HasOne(jobSeekerSkill => jobSeekerSkill.Skill)
                    .WithMany(skill => skill.JobSeekerSkills)
                    .HasForeignKey(jobSeekerSkill => jobSeekerSkill.SkillId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(jobSeekerSkill => new
                {
                    jobSeekerSkill.JobSeekerId,
                    jobSeekerSkill.SkillId
                }).IsUnique();
            });

            modelBuilder.Entity<Job>(entity =>
            {
                entity.ToTable("jobs");
                entity.HasKey(job => job.JobId);
                entity.Property(job => job.EmployerId).HasMaxLength(450);
                entity.Property(job => job.CompanyName).HasMaxLength(200).IsRequired();
                entity.Property(job => job.JobTitle).HasMaxLength(200).IsRequired();
                entity.Property(job => job.JobDescription).HasColumnType("nvarchar(max)").IsRequired();
                entity.Property(job => job.Responsibilities).HasColumnType("nvarchar(max)");
                entity.Property(job => job.MinimumQualification).HasMaxLength(150);
                entity.Property(job => job.PreferredFieldOfStudy).HasMaxLength(150);
                entity.Property(job => job.MinimumExperienceYears).HasPrecision(4, 1);
                entity.Property(job => job.EmploymentType).HasMaxLength(50).IsRequired();
                entity.Property(job => job.WorkplaceType).HasMaxLength(30);
                entity.Property(job => job.Location).HasMaxLength(200);
                entity.Property(job => job.MinimumSalary).HasPrecision(12, 2);
                entity.Property(job => job.MaximumSalary).HasPrecision(12, 2);
                entity.Property(job => job.ApplicationDeadline).HasColumnType("date");
                entity.Property(job => job.ApprovalStatus).HasMaxLength(30).HasDefaultValue("APPROVED");
                entity.Property(job => job.JobStatus).HasMaxLength(30).HasDefaultValue("OPEN");
                entity.Property(job => job.RejectionReason).HasColumnType("nvarchar(max)");
                entity.Property(job => job.ApprovedBy).HasMaxLength(450);
                entity.Property(job => job.ApprovedAt).HasColumnType("datetime2");
                entity.Property(job => job.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(job => job.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.HasOne(job => job.Employer)
                    .WithMany()
                    .HasForeignKey(job => job.EmployerId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(job => job.Approver)
                    .WithMany()
                    .HasForeignKey(job => job.ApprovedBy)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasIndex(job => new { job.JobStatus, job.ApprovalStatus });
                entity.HasIndex(job => job.Location);
            });

            modelBuilder.Entity<JobRequiredSkill>(entity =>
            {
                entity.ToTable("job_required_skills");
                entity.HasKey(required => required.JobRequiredSkillId);
                entity.Property(required => required.RequirementType).HasMaxLength(30).IsRequired();
                entity.Property(required => required.ImportanceWeight).HasPrecision(5, 2);

                entity.HasOne(required => required.Job)
                    .WithMany(job => job.RequiredSkills)
                    .HasForeignKey(required => required.JobId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(required => required.Skill)
                    .WithMany(skill => skill.JobRequiredSkills)
                    .HasForeignKey(required => required.SkillId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(required => new { required.JobId, required.SkillId }).IsUnique();
            });

            modelBuilder.Entity<JobApplication>(entity =>
            {
                entity.ToTable("job_applications");
                entity.HasKey(application => application.ApplicationId);
                entity.Property(application => application.JobSeekerId).HasMaxLength(450).IsRequired();
                entity.Property(application => application.CoverLetter).HasColumnType("nvarchar(max)");
                entity.Property(application => application.MatchPercentageAtApplication).HasPrecision(5, 2);
                entity.Property(application => application.ApplicationStatus).HasMaxLength(30).IsRequired().HasDefaultValue("SUBMITTED");
                entity.Property(application => application.EmployerNotes).HasColumnType("nvarchar(max)");
                entity.Property(application => application.AppliedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(application => application.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.HasOne(application => application.Job)
                    .WithMany(job => job.Applications)
                    .HasForeignKey(application => application.JobId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(application => application.Resume)
                    .WithMany(resume => resume.JobApplications)
                    .HasForeignKey(application => application.ResumeId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(application => new { application.JobId, application.JobSeekerId }).IsUnique();
                entity.HasIndex(application => new { application.JobSeekerId, application.ApplicationStatus });
            });

            modelBuilder.Entity<EmployerProfile>(entity =>
            {
                entity.ToTable("employer_profiles");
                entity.HasKey(e => e.EmployerId);
                entity.Property(e => e.EmployerId).HasMaxLength(450);
                entity.Property(e => e.CompanyName).HasMaxLength(200).IsRequired();
                entity.Property(e => e.CompanyRegistrationNumber).HasMaxLength(100);
                entity.Property(e => e.Industry).HasMaxLength(150);
                entity.Property(e => e.CompanySize).HasMaxLength(50);
                entity.Property(e => e.CompanyDescription).HasColumnType("nvarchar(max)");
                entity.Property(e => e.CompanyWebsite).HasColumnType("nvarchar(max)");
                entity.Property(e => e.CompanyAddress).HasColumnType("nvarchar(max)");
                entity.Property(e => e.CompanyLogoS3Key).HasMaxLength(1024);
                entity.Property(e => e.VerificationStatus).HasMaxLength(30).HasDefaultValue("PENDING");
                entity.Property(e => e.VerificationRemarks).HasColumnType("nvarchar(max)");
                entity.Property(e => e.VerifiedBy).HasMaxLength(450);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

                // 1-to-1 with ApplicationUser
                entity.HasOne(e => e.User)
                    .WithOne()
                    .HasForeignKey<EmployerProfile>(e => e.EmployerId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Admin who verified (no cascade — admin shouldn't delete the record)
                entity.HasOne(e => e.Verifier)
                    .WithMany()
                    .HasForeignKey(e => e.VerifiedBy)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<AuditLog>(entity =>
            {
                entity.ToTable("audit_logs");
                entity.HasKey(a => a.AuditLogId);
                entity.Property(a => a.UserId).HasMaxLength(450);
                entity.Property(a => a.ActionType).HasMaxLength(100).IsRequired();
                entity.Property(a => a.EntityType).HasMaxLength(100);
                entity.Property(a => a.ActionDescription).HasColumnType("nvarchar(max)");
                entity.Property(a => a.IpAddress).HasMaxLength(45);
                entity.Property(a => a.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

                // SET NULL so logs survive if user is deleted
                entity.HasOne(a => a.User)
                    .WithMany()
                    .HasForeignKey(a => a.UserId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasIndex(a => a.UserId);
                entity.HasIndex(a => a.CreatedAt);
            });

            modelBuilder.Entity<SystemReport>(entity =>
            {
                entity.ToTable("system_reports");
                entity.HasKey(r => r.SystemReportId);
                entity.Property(r => r.GeneratedBy).HasMaxLength(450).IsRequired();
                entity.Property(r => r.ReportName).HasMaxLength(200).IsRequired();
                entity.Property(r => r.ReportType).HasMaxLength(100).IsRequired();
                entity.Property(r => r.ReportParameters).HasColumnType("nvarchar(max)");
                entity.Property(r => r.OriginalFileName).HasMaxLength(255);
                entity.Property(r => r.ReportS3Key).HasMaxLength(1024);
                entity.Property(r => r.FileContentType).HasMaxLength(100);
                entity.Property(r => r.GeneratedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.HasOne(r => r.Generator)
                    .WithMany()
                    .HasForeignKey(r => r.GeneratedBy)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(r => r.GeneratedBy);
                entity.HasIndex(r => r.GeneratedAt);
            });
        }
    }
}