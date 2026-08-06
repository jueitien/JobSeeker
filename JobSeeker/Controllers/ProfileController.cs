using JobSeeker.Data;
using JobSeeker.Models;
using JobSeeker.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JobSeeker.Controllers
{
    [Authorize(Roles = UserRoles.JobSeeker)]
    public class ProfileController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ProfileController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await GetCurrentUserAsync();

            var profile = await _context.JobSeekerProfiles
                .Include(item => item.Resumes)
                .Include(item => item.Certifications)
                .Include(item => item.JobSeekerSkills)
                    .ThenInclude(item => item.Skill)
                .FirstOrDefaultAsync(item => item.JobSeekerId == user.Id);

            var viewModel = new JobSeekerProfileDetailsViewModel
            {
                Profile = profile,
                Resumes = profile?.Resumes
                    .OrderByDescending(item => item.IsPrimary)
                    .ThenByDescending(item => item.UploadedAt)
                    .ToList() ?? new List<Resume>(),
                Certifications = profile?.Certifications
                    .OrderByDescending(item => item.UploadedAt)
                    .ToList() ?? new List<Certification>(),
                Skills = profile?.JobSeekerSkills
                    .OrderBy(item => item.Skill.SkillName)
                    .ToList() ?? new List<JobSeekerSkill>()
            };

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            var user = await GetCurrentUserAsync();

            var profile = await _context.JobSeekerProfiles
                .FirstOrDefaultAsync(item => item.JobSeekerId == user.Id);

            return View(profile ?? new JobSeekerProfile
            {
                JobSeekerId = user.Id
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(JobSeekerProfile model)
        {
            var user = await GetCurrentUserAsync();

            if (!string.IsNullOrWhiteSpace(model.JobSeekerId)
                && !string.Equals(model.JobSeekerId, user.Id, StringComparison.Ordinal))
            {
                return Forbid();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var profile = await _context.JobSeekerProfiles
                .FirstOrDefaultAsync(item => item.JobSeekerId == user.Id);

            var now = DateTime.UtcNow;

            if (profile == null)
            {
                profile = new JobSeekerProfile
                {
                    JobSeekerId = user.Id,
                    CreatedAt = now
                };

                _context.JobSeekerProfiles.Add(profile);
            }

            profile.ProfileDescription = Normalize(model.ProfileDescription);
            profile.CareerObjective = Normalize(model.CareerObjective);
            profile.HighestQualification = Normalize(model.HighestQualification);
            profile.FieldOfStudy = Normalize(model.FieldOfStudy);
            profile.UniversityName = Normalize(model.UniversityName);
            profile.GraduationYear = model.GraduationYear;
            profile.ExperienceDescription = Normalize(model.ExperienceDescription);
            profile.PreferredJobTitle = Normalize(model.PreferredJobTitle);
            profile.PreferredLocation = Normalize(model.PreferredLocation);
            profile.ExpectedSalary = model.ExpectedSalary;
            profile.AvailabilityDate = model.AvailabilityDate;
            profile.UpdatedAt = now;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Your profile has been saved.";

            return RedirectToAction(nameof(Index));
        }

        private async Task<ApplicationUser> GetCurrentUserAsync()
        {
            return await _userManager.GetUserAsync(User)
                ?? throw new InvalidOperationException(
                    "The current user could not be loaded.");
        }

        private static string? Normalize(string? value)
        {
            var trimmed = value?.Trim();

            return string.IsNullOrWhiteSpace(trimmed) ? null : trimmed;
        }
    }
}