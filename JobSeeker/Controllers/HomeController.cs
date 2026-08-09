using JobSeeker.Data;
using JobSeeker.Models;
using JobSeeker.Models.ViewModels;
using JobSeeker.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JobSeeker.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [AllowAnonymous]
        public IActionResult Index()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                if (User.IsInRole(UserRoles.JobSeeker)) return RedirectToAction(nameof(JobSeeker));
                if (User.IsInRole(UserRoles.Employer)) return RedirectToAction(nameof(Employer));
                if (User.IsInRole(UserRoles.CareerCounsellor)) return RedirectToAction(nameof(CareerCounsellor));
                if (User.IsInRole(UserRoles.Administrator)) return RedirectToAction(nameof(Administrator));
                return Forbid();
            }

            return View();
        }

        [Authorize(Roles = UserRoles.JobSeeker)]
        public async Task<IActionResult> JobSeeker(
            string? keyword,
            string? location,
            string? employmentType,
            string? workplaceType,
            decimal? minimumSalary,
            string sort = "match")
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var profile = await _context.JobSeekerProfiles
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.JobSeekerId == user.Id);

            var seekerSkillIds = (await _context.JobSeekerSkills
                    .AsNoTracking()
                    .Where(x => x.JobSeekerId == user.Id)
                    .Select(x => x.SkillId)
                    .ToListAsync())
                .ToHashSet();

            var appliedJobIds = (await _context.JobApplications
                    .AsNoTracking()
                    .Where(x => x.JobSeekerId == user.Id)
                    .Select(x => x.JobId)
                    .ToListAsync())
                .ToHashSet();

            var allOpenJobs = await _context.Jobs
                .AsNoTracking()
                .Include(x => x.RequiredSkills)
                    .ThenInclude(x => x.Skill)
                .Where(x => x.ApprovalStatus == "APPROVED" && x.JobStatus == "OPEN")
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();

            var availableLocations = allOpenJobs
                .Where(x => !string.IsNullOrWhiteSpace(x.Location))
                .Select(x => x.Location!)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(x => x)
                .ToList();

            var availableEmploymentTypes = allOpenJobs
                .Select(x => x.EmploymentType)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(x => x)
                .ToList();

            var availableWorkplaceTypes = allOpenJobs
                .Where(x => !string.IsNullOrWhiteSpace(x.WorkplaceType))
                .Select(x => x.WorkplaceType!)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(x => x)
                .ToList();

            // Once a job seeker has applied, keep that job out of the discovery list.
            // The application remains available from My Applications for status tracking.
            IEnumerable<Job> filteredJobs = allOpenJobs
                .Where(job => !appliedJobIds.Contains(job.JobId));

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var term = keyword.Trim();
                filteredJobs = filteredJobs.Where(job =>
                    job.JobTitle.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                    job.CompanyName.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                    job.JobDescription.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                    job.RequiredSkills.Any(x => x.Skill.SkillName.Contains(term, StringComparison.OrdinalIgnoreCase)));
            }

            if (!string.IsNullOrWhiteSpace(location))
            {
                filteredJobs = filteredJobs.Where(job =>
                    job.Location != null &&
                    job.Location.Contains(location.Trim(), StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(employmentType))
            {
                filteredJobs = filteredJobs.Where(job =>
                    string.Equals(job.EmploymentType, employmentType, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(workplaceType))
            {
                filteredJobs = filteredJobs.Where(job =>
                    string.Equals(job.WorkplaceType, workplaceType, StringComparison.OrdinalIgnoreCase));
            }

            if (minimumSalary.HasValue)
            {
                filteredJobs = filteredJobs.Where(job =>
                    (job.MaximumSalary ?? job.MinimumSalary ?? 0) >= minimumSalary.Value);
            }

            var items = filteredJobs.Select(job =>
            {
                var result = JobMatchCalculator.Calculate(profile, seekerSkillIds, job);
                return new JobListingItemViewModel
                {
                    Job = job,
                    MatchPercentage = result.Percentage,
                    MatchedSkillCount = result.MatchedSkillCount,
                    RequiredSkillCount = result.RequiredSkillCount,
                    MatchedSkills = result.MatchedSkills,
                    MissingSkills = result.MissingSkills,
                    HasApplied = false,
                    ApplicationStatus = null
                };
            }).ToList();

            items = sort switch
            {
                "newest" => items.OrderByDescending(x => x.Job.CreatedAt).ToList(),
                "salary" => items.OrderByDescending(x => x.Job.MaximumSalary ?? x.Job.MinimumSalary ?? 0).ToList(),
                "title" => items.OrderBy(x => x.Job.JobTitle).ToList(),
                _ => items.OrderByDescending(x => x.MatchPercentage).ThenByDescending(x => x.Job.CreatedAt).ToList()
            };

            var viewModel = new JobSeekerHomeViewModel
            {
                Keyword = keyword,
                Location = location,
                EmploymentType = employmentType,
                WorkplaceType = workplaceType,
                MinimumSalary = minimumSalary,
                Sort = sort,
                Jobs = items,
                AvailableLocations = availableLocations,
                AvailableEmploymentTypes = availableEmploymentTypes,
                AvailableWorkplaceTypes = availableWorkplaceTypes
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = UserRoles.JobSeeker)]
        public async Task<IActionResult> GenerateTestJobs()
        {
            if (await _context.Jobs.AnyAsync(x => x.IsTestData))
            {
                TempData["InfoMessage"] = "The 5 sample jobs are already available.";
                return RedirectToAction(nameof(JobSeeker));
            }

            var requiredSkillNames = new[]
            {
                "C#", "ASP.NET Core", "SQL Server", "Git", "Problem Solving",
                "AWS", "Docker", "Python", "SQL", "Communication",
                "Microsoft Excel", "Power BI", "Data Analysis",
                "JavaScript", "HTML", "CSS", "React", "GitHub", "REST API"
            };

            var existingSkills = await _context.Skills
                .Where(x => requiredSkillNames.Contains(x.SkillName))
                .ToListAsync();

            var existingNames = existingSkills
                .Select(x => x.SkillName)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            foreach (var skillName in requiredSkillNames.Where(x => !existingNames.Contains(x)))
            {
                var skill = new Skill
                {
                    SkillName = skillName,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Skills.Add(skill);
                existingSkills.Add(skill);
            }

            await _context.SaveChangesAsync();
            var skillLookup = existingSkills.ToDictionary(x => x.SkillName, StringComparer.OrdinalIgnoreCase);
            var now = DateTime.UtcNow;

            var jobs = new List<Job>
            {
                new()
                {
                    CompanyName = "Sabah Digital Works Sdn Bhd",
                    JobTitle = "Junior .NET Developer",
                    JobDescription = "Build and maintain web applications using C#, ASP.NET Core and SQL Server. Suitable for fresh graduates with practical software development experience.",
                    Responsibilities = "Develop web features, fix bugs, work with databases and collaborate using Git.",
                    MinimumQualification = "Bachelor Degree",
                    PreferredFieldOfStudy = "Software Engineering",
                    MinimumExperienceYears = 0,
                    EmploymentType = "FULL_TIME",
                    WorkplaceType = "HYBRID",
                    Location = "Kota Kinabalu, Sabah",
                    MinimumSalary = 2800,
                    MaximumSalary = 3800,
                    VacancyCount = 2,
                    ApplicationDeadline = DateOnly.FromDateTime(now.AddDays(30)),
                    ApprovalStatus = "APPROVED",
                    JobStatus = "OPEN",
                    IsTestData = true,
                    CreatedAt = now,
                    UpdatedAt = now
                },
                new()
                {
                    CompanyName = "Borneo Cloud Solutions",
                    JobTitle = "Cloud Support Associate",
                    JobDescription = "Support cloud workloads, troubleshoot deployment issues and assist customers using AWS services and container technologies.",
                    Responsibilities = "Monitor cloud services, support deployments, document issues and assist technical users.",
                    MinimumQualification = "Diploma",
                    PreferredFieldOfStudy = "Information Technology",
                    MinimumExperienceYears = 0,
                    EmploymentType = "FULL_TIME",
                    WorkplaceType = "ON_SITE",
                    Location = "Penampang, Sabah",
                    MinimumSalary = 2600,
                    MaximumSalary = 3600,
                    VacancyCount = 2,
                    ApplicationDeadline = DateOnly.FromDateTime(now.AddDays(35)),
                    ApprovalStatus = "APPROVED",
                    JobStatus = "OPEN",
                    IsTestData = true,
                    CreatedAt = now.AddMinutes(-10),
                    UpdatedAt = now
                },
                new()
                {
                    CompanyName = "Insight Data Lab",
                    JobTitle = "Junior Data Analyst",
                    JobDescription = "Prepare reports and analyse operational data using SQL, Python, Microsoft Excel and Power BI.",
                    Responsibilities = "Clean data, create dashboards, prepare reports and explain findings to the team.",
                    MinimumQualification = "Bachelor Degree",
                    PreferredFieldOfStudy = "Information Technology",
                    MinimumExperienceYears = 0,
                    EmploymentType = "FULL_TIME",
                    WorkplaceType = "HYBRID",
                    Location = "Kota Kinabalu, Sabah",
                    MinimumSalary = 3000,
                    MaximumSalary = 4200,
                    VacancyCount = 1,
                    ApplicationDeadline = DateOnly.FromDateTime(now.AddDays(28)),
                    ApprovalStatus = "APPROVED",
                    JobStatus = "OPEN",
                    IsTestData = true,
                    CreatedAt = now.AddMinutes(-20),
                    UpdatedAt = now
                },
                new()
                {
                    CompanyName = "PixelCraft Studio",
                    JobTitle = "Frontend Developer",
                    JobDescription = "Create responsive user interfaces using JavaScript, HTML, CSS and React while collaborating through GitHub.",
                    Responsibilities = "Build reusable UI components, implement responsive layouts and work with backend REST APIs.",
                    MinimumQualification = "Diploma",
                    PreferredFieldOfStudy = "Software Engineering",
                    MinimumExperienceYears = 0,
                    EmploymentType = "FULL_TIME",
                    WorkplaceType = "REMOTE",
                    Location = "Remote - Malaysia",
                    MinimumSalary = 2800,
                    MaximumSalary = 4000,
                    VacancyCount = 2,
                    ApplicationDeadline = DateOnly.FromDateTime(now.AddDays(32)),
                    ApprovalStatus = "APPROVED",
                    JobStatus = "OPEN",
                    IsTestData = true,
                    CreatedAt = now.AddMinutes(-30),
                    UpdatedAt = now
                },
                new()
                {
                    CompanyName = "North Borneo Tech Services",
                    JobTitle = "Junior Software Engineer",
                    JobDescription = "Join a software team building business applications and APIs. The role values C#, SQL, REST API knowledge, GitHub and strong problem solving.",
                    Responsibilities = "Develop application modules, test features, maintain APIs and participate in code reviews.",
                    MinimumQualification = "Bachelor Degree",
                    PreferredFieldOfStudy = "Software Engineering",
                    MinimumExperienceYears = 0,
                    EmploymentType = "FULL_TIME",
                    WorkplaceType = "ON_SITE",
                    Location = "Kota Kinabalu, Sabah",
                    MinimumSalary = 3000,
                    MaximumSalary = 4300,
                    VacancyCount = 3,
                    ApplicationDeadline = DateOnly.FromDateTime(now.AddDays(40)),
                    ApprovalStatus = "APPROVED",
                    JobStatus = "OPEN",
                    IsTestData = true,
                    CreatedAt = now.AddMinutes(-40),
                    UpdatedAt = now
                }
            };

            _context.Jobs.AddRange(jobs);
            await _context.SaveChangesAsync();

            AddRequiredSkills(jobs[0], skillLookup, ("C#", 25), ("ASP.NET Core", 25), ("SQL Server", 20), ("Git", 15), ("Problem Solving", 15));
            AddRequiredSkills(jobs[1], skillLookup, ("AWS", 35), ("Docker", 20), ("SQL", 15), ("Git", 15), ("Communication", 15));
            AddRequiredSkills(jobs[2], skillLookup, ("SQL", 25), ("Python", 25), ("Microsoft Excel", 20), ("Power BI", 20), ("Data Analysis", 10));
            AddRequiredSkills(jobs[3], skillLookup, ("JavaScript", 25), ("HTML", 15), ("CSS", 15), ("React", 25), ("GitHub", 20));
            AddRequiredSkills(jobs[4], skillLookup, ("C#", 20), ("SQL", 20), ("REST API", 20), ("GitHub", 15), ("Problem Solving", 25));

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "5 sample jobs were created. Their match scores are calculated from your current profile and skills.";
            return RedirectToAction(nameof(JobSeeker));
        }

        private void AddRequiredSkills(Job job, Dictionary<string, Skill> skills, params (string Name, decimal Weight)[] requiredSkills)
        {
            foreach (var item in requiredSkills)
            {
                _context.JobRequiredSkills.Add(new JobRequiredSkill
                {
                    JobId = job.JobId,
                    SkillId = skills[item.Name].SkillId,
                    RequirementType = "REQUIRED",
                    ImportanceWeight = item.Weight
                });
            }
        }

        [Authorize(Roles = UserRoles.Employer)]
        public IActionResult Employer() => View();

        [Authorize(Roles = UserRoles.CareerCounsellor)]
        public async Task<IActionResult> CareerCounsellor()
        {
            var viewModel = new CareerCounsellorHomeViewModel
            {
                ResumeReviewCount = await _context.ResumeFeedback.CountAsync(),
                CareerRecommendationCount = await _context.CareerRecommendations.CountAsync(),
                SkillRecommendationCount = await _context.SkillRecommendations.CountAsync(),
                CertificationRecommendationCount = await _context.CertificationRecommendations.CountAsync(),
                RecentResumeReviews = await _context.ResumeFeedback
                    .AsNoTracking()
                    .Include(review => review.Resume)
                        .ThenInclude(resume => resume.JobSeekerProfile)
                        .ThenInclude(profile => profile.User)
                    .OrderByDescending(review => review.UpdatedAt)
                    .Take(5)
                    .Select(review => new RecentResumeReviewViewModel
                    {
                        JobSeekerName = review.Resume.JobSeekerProfile.User.FullName,
                        ResumeTitle = review.Resume.ResumeTitle ?? "Untitled resume",
                        FeedbackStatus = review.FeedbackStatus,
                        UpdatedAt = review.UpdatedAt
                    })
                    .ToListAsync()
            };

            return View(viewModel);
        }

        [Authorize(Roles = UserRoles.Administrator)]
        public IActionResult Administrator() => View();
    }
}
