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
                .Include(x => x.VacancyImages)
                .Where(x => x.ApprovalStatus == "APPROVED" && x.JobStatus == "OPEN" && !x.IsTestData)
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
