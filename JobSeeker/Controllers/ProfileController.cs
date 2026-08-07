using JobSeeker.Data;
using JobSeeker.Models;
using JobSeeker.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace JobSeeker.Controllers
{
    [Authorize(Roles = UserRoles.JobSeeker)]
    public class ProfileController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _environment;

        public ProfileController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment environment)
        {
            _context = context;
            _userManager = userManager;
            _environment = environment;
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

            ViewBag.UserName = !string.IsNullOrWhiteSpace(user.FullName) ? user.FullName : (user.UserName ?? user.Email ?? "Job Seeker");
            ViewBag.Email = user.Email;
            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            var user = await GetCurrentUserAsync();

            var profile = await _context.JobSeekerProfiles
                .FirstOrDefaultAsync(item => item.JobSeekerId == user.Id);

            await PopulateEditManagementDataAsync(user.Id);

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

            // Navigation properties are not posted by the form.
            ModelState.Remove(nameof(JobSeekerProfile.User));
            ModelState.Remove(nameof(JobSeekerProfile.Resumes));
            ModelState.Remove(nameof(JobSeekerProfile.Certifications));
            ModelState.Remove(nameof(JobSeekerProfile.JobSeekerSkills));
            ModelState.Remove(nameof(JobSeekerProfile.JobApplications));

            if (!ModelState.IsValid)
            {
                await PopulateEditManagementDataAsync(user.Id);
                return View(model);
            }

            var profile = await EnsureProfileAsync(user.Id);
            var now = DateTime.UtcNow;

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

            TempData["SuccessMessage"] = "Your profile has been updated.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddSkill(string skillName, string? proficiencyLevel)
        {
            var user = await GetCurrentUserAsync();
            await EnsureProfileAsync(user.Id);

            skillName = skillName?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(skillName))
            {
                TempData["ErrorMessage"] = "Enter a skill name first.";
                return RedirectToAction(nameof(Edit));
            }

            var skill = await _context.Skills
                .FirstOrDefaultAsync(x => x.SkillName.ToLower() == skillName.ToLower());

            if (skill == null)
            {
                skill = new Skill
                {
                    SkillName = skillName,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Skills.Add(skill);
                await _context.SaveChangesAsync();
            }

            var existing = await _context.JobSeekerSkills
                .FirstOrDefaultAsync(x => x.JobSeekerId == user.Id && x.SkillId == skill.SkillId);

            if (existing == null)
            {
                _context.JobSeekerSkills.Add(new JobSeekerSkill
                {
                    JobSeekerId = user.Id,
                    SkillId = skill.SkillId,
                    ProficiencyLevel = Normalize(proficiencyLevel),
                    CreatedAt = DateTime.UtcNow
                });
            }
            else
            {
                existing.ProficiencyLevel = Normalize(proficiencyLevel);
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Skill saved.";
            return RedirectToAction(nameof(Edit));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveSkill(long id)
        {
            var user = await GetCurrentUserAsync();
            var skill = await _context.JobSeekerSkills
                .FirstOrDefaultAsync(x => x.JobSeekerSkillId == id && x.JobSeekerId == user.Id);

            if (skill != null)
            {
                _context.JobSeekerSkills.Remove(skill);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Edit));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadResume(string? resumeTitle, string? resumeDescription, IFormFile? resumeFile)
        {
            var user = await GetCurrentUserAsync();
            await EnsureProfileAsync(user.Id);

            if (resumeFile == null || resumeFile.Length == 0)
            {
                TempData["ErrorMessage"] = "Choose a resume file to upload.";
                return RedirectToAction(nameof(Edit));
            }

            var allowed = new[] { ".pdf", ".doc", ".docx" };
            var extension = Path.GetExtension(resumeFile.FileName).ToLowerInvariant();
            if (!allowed.Contains(extension))
            {
                TempData["ErrorMessage"] = "Resume must be PDF, DOC, or DOCX.";
                return RedirectToAction(nameof(Edit));
            }

            var relativePath = await SaveLocalFileAsync(resumeFile, "resumes", user.Id);
            var hasResume = await _context.Resumes.AnyAsync(x => x.JobSeekerId == user.Id);

            var normalizedDescription = Normalize(resumeDescription);

            _context.Resumes.Add(new Resume
            {
                JobSeekerId = user.Id,
                ResumeTitle = string.IsNullOrWhiteSpace(resumeTitle) ? Path.GetFileNameWithoutExtension(resumeFile.FileName) : resumeTitle.Trim(),
                ResumeDescription = normalizedDescription,
                ResumeS3Key = relativePath,
                IsPrimary = !hasResume,
                UploadedAt = DateTime.UtcNow
            });

            var detectedSkills = await AppendDetectedSkillsAsync(user.Id, normalizedDescription);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = detectedSkills.Count > 0
                ? $"Resume uploaded. Added skill(s): {string.Join(", ", detectedSkills)}."
                : "Resume uploaded. No existing skills were detected from the description.";
            return RedirectToAction(nameof(Edit));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetPrimaryResume(long id)
        {
            var user = await GetCurrentUserAsync();
            var resumes = await _context.Resumes.Where(x => x.JobSeekerId == user.Id).ToListAsync();
            var selected = resumes.FirstOrDefault(x => x.ResumeId == id);

            if (selected != null)
            {
                foreach (var resume in resumes)
                    resume.IsPrimary = resume.ResumeId == id;

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Primary resume updated.";
            }

            return RedirectToAction(nameof(Edit));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteResume(long id)
        {
            var user = await GetCurrentUserAsync();
            var resume = await _context.Resumes
                .FirstOrDefaultAsync(x => x.ResumeId == id && x.JobSeekerId == user.Id);

            if (resume != null)
            {
                var usedByApplication = await _context.JobApplications
                    .AnyAsync(x => x.ResumeId == resume.ResumeId);

                if (usedByApplication)
                {
                    TempData["ErrorMessage"] = "This resume is attached to a job application and cannot be deleted.";
                    return RedirectToAction(nameof(Edit));
                }

                DeleteLocalFile(resume.ResumeS3Key);
                _context.Resumes.Remove(resume);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Edit));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadCertification(string certificationName, string? description, IFormFile? certificateFile)
        {
            var user = await GetCurrentUserAsync();
            await EnsureProfileAsync(user.Id);

            if (string.IsNullOrWhiteSpace(certificationName) || certificateFile == null || certificateFile.Length == 0)
            {
                TempData["ErrorMessage"] = "Enter the certification name and choose a file.";
                return RedirectToAction(nameof(Edit));
            }

            var allowed = new[] { ".pdf", ".jpg", ".jpeg", ".png" };
            var extension = Path.GetExtension(certificateFile.FileName).ToLowerInvariant();
            if (!allowed.Contains(extension))
            {
                TempData["ErrorMessage"] = "Certificate must be PDF, JPG, JPEG, or PNG.";
                return RedirectToAction(nameof(Edit));
            }

            var relativePath = await SaveLocalFileAsync(certificateFile, "certifications", user.Id);

            var normalizedDescription = Normalize(description);

            _context.Certifications.Add(new Certification
            {
                JobSeekerId = user.Id,
                CertificationName = certificationName.Trim(),
                Description = normalizedDescription,
                CertificateS3Key = relativePath,
                UploadedAt = DateTime.UtcNow
            });

            var detectedSkills = await AppendDetectedSkillsAsync(user.Id, normalizedDescription);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = detectedSkills.Count > 0
                ? $"Certification uploaded. Added skill(s): {string.Join(", ", detectedSkills)}."
                : "Certification uploaded. No existing skills were detected from the description.";
            return RedirectToAction(nameof(Edit));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCertification(long id)
        {
            var user = await GetCurrentUserAsync();
            var certification = await _context.Certifications
                .FirstOrDefaultAsync(x => x.CertificationId == id && x.JobSeekerId == user.Id);

            if (certification != null)
            {
                DeleteLocalFile(certification.CertificateS3Key);
                _context.Certifications.Remove(certification);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Edit));
        }

        private async Task<List<string>> AppendDetectedSkillsAsync(string userId, string? description)
        {
            var detectedNames = new List<string>();

            if (string.IsNullOrWhiteSpace(description))
                return detectedNames;

            var availableSkills = await _context.Skills
                .AsNoTracking()
                .OrderByDescending(x => x.SkillName.Length)
                .ToListAsync();

            if (availableSkills.Count == 0)
                return detectedNames;

            var existingSkillIds = await _context.JobSeekerSkills
                .Where(x => x.JobSeekerId == userId)
                .Select(x => x.SkillId)
                .ToListAsync();

            var existingSkillIdSet = existingSkillIds.ToHashSet();

            foreach (var skill in availableSkills)
            {
                if (!ContainsSkill(description, skill.SkillName))
                    continue;

                detectedNames.Add(skill.SkillName);

                if (existingSkillIdSet.Contains(skill.SkillId))
                    continue;

                _context.JobSeekerSkills.Add(new JobSeekerSkill
                {
                    JobSeekerId = userId,
                    SkillId = skill.SkillId,
                    ProficiencyLevel = null,
                    CreatedAt = DateTime.UtcNow
                });

                existingSkillIdSet.Add(skill.SkillId);
            }

            return detectedNames.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        }

        private static bool ContainsSkill(string description, string skillName)
        {
            if (string.IsNullOrWhiteSpace(skillName))
                return false;

            // Match the complete skill name/phrase while still supporting names such as C#, C++, .NET and AWS.
            var pattern = $@"(?<![A-Za-z0-9]){Regex.Escape(skillName.Trim())}(?![A-Za-z0-9])";
            return Regex.IsMatch(description, pattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        }

        private async Task PopulateEditManagementDataAsync(string userId)
        {
            ViewBag.Resumes = await _context.Resumes
                .Where(x => x.JobSeekerId == userId)
                .OrderByDescending(x => x.IsPrimary)
                .ThenByDescending(x => x.UploadedAt)
                .ToListAsync();

            ViewBag.Certifications = await _context.Certifications
                .Where(x => x.JobSeekerId == userId)
                .OrderByDescending(x => x.UploadedAt)
                .ToListAsync();

            ViewBag.Skills = await _context.JobSeekerSkills
                .Where(x => x.JobSeekerId == userId)
                .Include(x => x.Skill)
                .OrderBy(x => x.Skill.SkillName)
                .ToListAsync();
        }

        private async Task<JobSeekerProfile> EnsureProfileAsync(string userId)
        {
            var profile = await _context.JobSeekerProfiles.FirstOrDefaultAsync(x => x.JobSeekerId == userId);
            if (profile != null) return profile;

            profile = new JobSeekerProfile
            {
                JobSeekerId = userId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _context.JobSeekerProfiles.Add(profile);
            await _context.SaveChangesAsync();
            return profile;
        }

        private async Task<string> SaveLocalFileAsync(IFormFile file, string folder, string userId)
        {
            var safeUser = string.Concat(userId.Select(ch => char.IsLetterOrDigit(ch) ? ch : '_'));
            var uploadFolder = Path.Combine(_environment.WebRootPath, "uploads", folder, safeUser);
            Directory.CreateDirectory(uploadFolder);

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var fileName = $"{Guid.NewGuid():N}{extension}";
            var fullPath = Path.Combine(uploadFolder, fileName);

            await using var stream = new FileStream(fullPath, FileMode.Create);
            await file.CopyToAsync(stream);

            return $"/uploads/{folder}/{safeUser}/{fileName}";
        }

        private void DeleteLocalFile(string? relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath) || !relativePath.StartsWith("/uploads/")) return;
            var local = relativePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
            var fullPath = Path.Combine(_environment.WebRootPath, local);
            if (System.IO.File.Exists(fullPath)) System.IO.File.Delete(fullPath);
        }

        private async Task<ApplicationUser> GetCurrentUserAsync()
        {
            return await _userManager.GetUserAsync(User)
                ?? throw new InvalidOperationException("The current user could not be loaded.");
        }

        private static string? Normalize(string? value)
        {
            var trimmed = value?.Trim();
            return string.IsNullOrWhiteSpace(trimmed) ? null : trimmed;
        }
    }
}
