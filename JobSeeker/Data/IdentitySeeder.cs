using JobSeeker.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace JobSeeker.Data
{
    public static class IdentitySeeder
    {
        public static async Task SeedRolesAsync(IServiceProvider services)
        {
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

            foreach (var roleName in UserRoles.All)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                    await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }

        public static async Task SeedAdminUserAsync(IServiceProvider services)
        {
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

            const string adminEmail    = "admin@jobseeker.com";
            const string adminPassword = "Admin@12345";

            if (await userManager.FindByEmailAsync(adminEmail) != null)
                return;

            var admin = new ApplicationUser
            {
                FullName       = "System Administrator",
                UserName       = adminEmail,
                Email          = adminEmail,
                EmailConfirmed = true,
                AccountStatus  = "ACTIVE",
                CreatedAt      = DateTime.UtcNow,
                UpdatedAt      = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(admin, adminPassword);
            if (result.Succeeded)
                await userManager.AddToRoleAsync(admin, UserRoles.Administrator);
            else
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new Exception($"Failed to seed admin user: {errors}");
            }
        }

        /// <summary>
        /// Seeds fake users, employer profiles, jobs and audit logs for
        /// development/testing of the admin panel. Safe to run multiple times.
        /// </summary>
        public static async Task SeedFakeAdminDataAsync(IServiceProvider services)
        {
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
            var db          = services.GetRequiredService<ApplicationDbContext>();

            // Skip if fake data already exists
            if (await db.EmployerProfiles.AnyAsync())
                return;

            var now = DateTime.UtcNow;

            // ── 1. Fake users ──────────────────────────────────────────────────
            var fakeUsers = new[]
            {
                ("Ahmad Razif",      "ahmad@jobseeker.com",   UserRoles.JobSeeker,       "ACTIVE"),
                ("Priya Nair",       "priya@jobseeker.com",   UserRoles.JobSeeker,       "ACTIVE"),
                ("Tan Wei Liang",    "tanwei@jobseeker.com",  UserRoles.JobSeeker,       "SUSPENDED"),
                ("Siti Nurhaliza",   "siti@employer.com",     UserRoles.Employer,        "ACTIVE"),
                ("Tech Corp Sdn Bhd","techcorp@employer.com", UserRoles.Employer,        "ACTIVE"),
                ("Borneo Cloud",     "borneo@employer.com",   UserRoles.Employer,        "ACTIVE"),
                ("Digital Talents",  "digital@employer.com",  UserRoles.Employer,        "ACTIVE"),
                ("Chua Mei Ling",    "chua@counsellor.com",   UserRoles.CareerCounsellor,"ACTIVE"),
            };

            var createdUsers = new Dictionary<string, ApplicationUser>();

            foreach (var (name, email, role, status) in fakeUsers)
            {
                if (await userManager.FindByEmailAsync(email) != null)
                    continue;

                var user = new ApplicationUser
                {
                    FullName       = name,
                    UserName       = email,
                    Email          = email,
                    EmailConfirmed = true,
                    AccountStatus  = status,
                    CreatedAt      = now.AddDays(-new Random().Next(1, 60)),
                    UpdatedAt      = now
                };

                var result = await userManager.CreateAsync(user, "Test@12345");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, role);
                    createdUsers[email] = user;
                }
            }

            // ── 2. Employer profiles ───────────────────────────────────────────
            var adminUser = await userManager.FindByEmailAsync("admin@jobseeker.com");

            var employerData = new[]
            {
                ("siti@employer.com",    "Siti HR Consulting",  "Human Resources", "1–10",   "APPROVED"),
                ("techcorp@employer.com","Tech Corp Sdn Bhd",   "Technology",      "51–200", "PENDING"),
                ("borneo@employer.com",  "Borneo Cloud Solutions","Technology",    "11–50",  "PENDING"),
                ("digital@employer.com", "Digital Talents MY",  "Recruitment",     "1–10",   "REJECTED"),
            };

            foreach (var (email, company, industry, size, status) in employerData)
            {
                var user = await userManager.FindByEmailAsync(email);
                if (user == null) continue;

                db.EmployerProfiles.Add(new EmployerProfile
                {
                    EmployerId               = user.Id,
                    CompanyName              = company,
                    Industry                 = industry,
                    CompanySize              = size,
                    CompanyRegistrationNumber = $"REG-{new Random().Next(100000, 999999)}",
                    VerificationStatus       = status,
                    VerificationRemarks      = status == "REJECTED" ? "Incomplete business registration documents." : null,
                    VerifiedBy               = status != "PENDING" ? adminUser?.Id : null,
                    VerifiedAt               = status != "PENDING" ? now.AddDays(-2) : null,
                    CreatedAt                = now.AddDays(-10),
                    UpdatedAt                = now
                });
            }

            await db.SaveChangesAsync();

            // ── 3. Fake jobs ───────────────────────────────────────────────────
            var techCorpUser = await userManager.FindByEmailAsync("techcorp@employer.com");
            var borneoUser   = await userManager.FindByEmailAsync("borneo@employer.com");

            var fakeJobs = new[]
            {
                new Job
                {
                    EmployerId           = techCorpUser?.Id,
                    CompanyName          = "Tech Corp Sdn Bhd",
                    JobTitle             = "Senior .NET Developer",
                    JobDescription       = "Build enterprise web applications using ASP.NET Core.",
                    MinimumQualification = "Bachelor Degree",
                    MinimumExperienceYears = 3,
                    EmploymentType       = "FULL_TIME",
                    WorkplaceType        = "HYBRID",
                    Location             = "Kota Kinabalu, Sabah",
                    MinimumSalary        = 5000,
                    MaximumSalary        = 8000,
                    VacancyCount         = 2,
                    ApplicationDeadline  = DateOnly.FromDateTime(now.AddDays(30)),
                    ApprovalStatus       = "PENDING",
                    JobStatus            = "CLOSED",
                    CreatedAt            = now.AddHours(-5),
                    UpdatedAt            = now
                },
                new Job
                {
                    EmployerId           = borneoUser?.Id,
                    CompanyName          = "Borneo Cloud Solutions",
                    JobTitle             = "Cloud Infrastructure Engineer",
                    JobDescription       = "Manage AWS and Azure cloud workloads for enterprise clients.",
                    MinimumQualification = "Diploma",
                    MinimumExperienceYears = 1,
                    EmploymentType       = "FULL_TIME",
                    WorkplaceType        = "REMOTE",
                    Location             = "Remote – Malaysia",
                    MinimumSalary        = 4000,
                    MaximumSalary        = 6500,
                    VacancyCount         = 1,
                    ApplicationDeadline  = DateOnly.FromDateTime(now.AddDays(20)),
                    ApprovalStatus       = "PENDING",
                    JobStatus            = "CLOSED",
                    CreatedAt            = now.AddHours(-2),
                    UpdatedAt            = now
                },
                new Job
                {
                    EmployerId           = techCorpUser?.Id,
                    CompanyName          = "Tech Corp Sdn Bhd",
                    JobTitle             = "UI/UX Designer",
                    JobDescription       = "Design user interfaces for our SaaS products.",
                    MinimumQualification = "Diploma",
                    MinimumExperienceYears = 0,
                    EmploymentType       = "FULL_TIME",
                    WorkplaceType        = "ON_SITE",
                    Location             = "Kota Kinabalu, Sabah",
                    MinimumSalary        = 3000,
                    MaximumSalary        = 4500,
                    VacancyCount         = 1,
                    ApprovalStatus       = "APPROVED",
                    JobStatus            = "OPEN",
                    ApprovedBy           = adminUser?.Id,
                    ApprovedAt           = now.AddDays(-3),
                    CreatedAt            = now.AddDays(-5),
                    UpdatedAt            = now
                },
                new Job
                {
                    EmployerId           = borneoUser?.Id,
                    CompanyName          = "Borneo Cloud Solutions",
                    JobTitle             = "Junior Data Analyst",
                    JobDescription       = "Analyse business data and produce reports.",
                    MinimumQualification = "Bachelor Degree",
                    MinimumExperienceYears = 0,
                    EmploymentType       = "FULL_TIME",
                    WorkplaceType        = "HYBRID",
                    Location             = "Penampang, Sabah",
                    MinimumSalary        = 2800,
                    MaximumSalary        = 3800,
                    VacancyCount         = 2,
                    ApprovalStatus       = "REJECTED",
                    JobStatus            = "CLOSED",
                    RejectionReason      = "Job description does not meet platform standards. Please revise.",
                    ApprovedBy           = adminUser?.Id,
                    ApprovedAt           = now.AddDays(-1),
                    CreatedAt            = now.AddDays(-4),
                    UpdatedAt            = now
                },
            };

            db.Jobs.AddRange(fakeJobs);
            await db.SaveChangesAsync();

            // ── 4. Audit logs ──────────────────────────────────────────────────
            db.AuditLogs.AddRange(
                new AuditLog
                {
                    UserId            = adminUser?.Id,
                    ActionType        = "EMPLOYER_APPROVED",
                    EntityType        = "EmployerProfile",
                    ActionDescription = "Approved employer: Siti HR Consulting",
                    CreatedAt         = now.AddDays(-2)
                },
                new AuditLog
                {
                    UserId            = adminUser?.Id,
                    ActionType        = "EMPLOYER_REJECTED",
                    EntityType        = "EmployerProfile",
                    ActionDescription = "Rejected employer: Digital Talents MY. Reason: Incomplete documents.",
                    CreatedAt         = now.AddDays(-2).AddHours(1)
                },
                new AuditLog
                {
                    UserId            = adminUser?.Id,
                    ActionType        = "JOB_APPROVED",
                    EntityType        = "Job",
                    ActionDescription = "Approved job: UI/UX Designer at Tech Corp Sdn Bhd",
                    CreatedAt         = now.AddDays(-3)
                },
                new AuditLog
                {
                    UserId            = adminUser?.Id,
                    ActionType        = "JOB_REJECTED",
                    EntityType        = "Job",
                    ActionDescription = "Rejected job: Junior Data Analyst at Borneo Cloud Solutions.",
                    CreatedAt         = now.AddDays(-1)
                },
                new AuditLog
                {
                    UserId            = adminUser?.Id,
                    ActionType        = "USER_SUSPENDED",
                    EntityType        = "User",
                    ActionDescription = "Suspended user: Tan Wei Liang (tanwei@jobseeker.com)",
                    CreatedAt         = now.AddDays(-5)
                }
            );

            await db.SaveChangesAsync();
        }
    }
}
