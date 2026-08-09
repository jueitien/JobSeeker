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
    public class FeedbackController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public FeedbackController(
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

            var resumes = await _context.Resumes
                .AsNoTracking()
                .Where(x => x.JobSeekerId == user.Id)
                .OrderByDescending(x => x.IsPrimary)
                .ThenByDescending(x => x.UploadedAt)
                .ToListAsync();

            var resumeFeedback = await _context.ResumeFeedback
                .AsNoTracking()
                .Include(x => x.Resume)
                .Include(x => x.Counsellor)
                .Where(x => x.Resume.JobSeekerId == user.Id)
                .ToListAsync();

            var careerRecommendations = await _context.CareerRecommendations
                .AsNoTracking()
                .Include(x => x.Counsellor)
                .Where(x => x.JobSeekerId == user.Id)
                .ToListAsync();

            var skillRecommendations = await _context.SkillRecommendations
                .AsNoTracking()
                .Include(x => x.Counsellor)
                .Where(x => x.JobSeekerId == user.Id)
                .ToListAsync();

            var certificationRecommendations = await _context.CertificationRecommendations
                .AsNoTracking()
                .Include(x => x.Counsellor)
                .Where(x => x.JobSeekerId == user.Id)
                .ToListAsync();

            var requests = new List<FeedbackRequestItemViewModel>();

            requests.AddRange(resumeFeedback.Select(x => new FeedbackRequestItemViewModel
            {
                Id = x.ResumeFeedbackId,
                Type = "Resume Feedback",
                Title = string.IsNullOrWhiteSpace(x.Resume.ResumeTitle) ? "Resume review" : x.Resume.ResumeTitle!,
                RequestMessage = x.RequestMessage,
                Status = x.FeedbackStatus,
                CreatedAt = x.CreatedAt,
                CounsellorName = x.Counsellor?.FullName,
                ResponseTitle = x.FeedbackStatus == "COMPLETED" ? "Counsellor feedback" : null,
                ResponseBody = x.FeedbackStatus == "COMPLETED" ? x.OverallComment : null,
                ResponseDetails = x.FeedbackStatus == "COMPLETED"
                    ? JoinResponseParts(
                        ("Strengths", x.Strengths),
                        ("Areas to improve", x.Weaknesses),
                        ("Recommended changes", x.RecommendedChanges))
                    : null
            }));

            requests.AddRange(careerRecommendations.Select(x => new FeedbackRequestItemViewModel
            {
                Id = x.CareerRecommendationId,
                Type = "Career Advice",
                Title = "Career recommendation",
                RequestMessage = x.RequestMessage,
                Status = x.RecommendationStatus,
                CreatedAt = x.CreatedAt,
                CounsellorName = x.Counsellor?.FullName,
                ResponseTitle = x.RecommendationStatus == "COMPLETED" && !string.IsNullOrWhiteSpace(x.RecommendedJobTitle)
                    ? x.RecommendedJobTitle
                    : null,
                ResponseBody = x.RecommendationStatus == "COMPLETED" ? x.RecommendationReason : null,
                ResponseDetails = x.RecommendationStatus == "COMPLETED"
                    ? JoinResponseParts(
                        ("Industry", x.RecommendedIndustry),
                        ("Improvements", x.RequiredImprovements))
                    : null
            }));

            requests.AddRange(skillRecommendations.Select(x => new FeedbackRequestItemViewModel
            {
                Id = x.SkillRecommendationId,
                Type = "Skill Recommendation",
                Title = "Skill development advice",
                RequestMessage = x.RequestMessage,
                Status = x.RecommendationStatus,
                CreatedAt = x.CreatedAt,
                CounsellorName = x.Counsellor?.FullName,
                ResponseTitle = x.RecommendationStatus == "COMPLETED" ? x.RecommendedSkill : null,
                ResponseBody = x.RecommendationStatus == "COMPLETED" ? x.RecommendationReason : null,
                ResponseDetails = x.RecommendationStatus == "COMPLETED" && !string.IsNullOrWhiteSpace(x.PriorityLevel)
                    ? $"Priority: {Friendly(x.PriorityLevel)}"
                    : null
            }));

            requests.AddRange(certificationRecommendations.Select(x => new FeedbackRequestItemViewModel
            {
                Id = x.CertificationRecommendationId,
                Type = "Certification Recommendation",
                Title = "Certification advice",
                RequestMessage = x.RequestMessage,
                Status = x.RecommendationStatus,
                CreatedAt = x.CreatedAt,
                CounsellorName = x.Counsellor?.FullName,
                ResponseTitle = x.RecommendationStatus == "COMPLETED" ? x.CertificationName : null,
                ResponseBody = x.RecommendationStatus == "COMPLETED" ? x.RecommendationReason : null,
                ResponseDetails = x.RecommendationStatus == "COMPLETED"
                    ? JoinResponseParts(
                        ("Issuing organisation", x.IssuingOrganization),
                        ("Priority", Friendly(x.PriorityLevel)))
                    : null
            }));

            var mySkillIds = (await _context.JobSeekerSkills
                    .AsNoTracking()
                    .Where(x => x.JobSeekerId == user.Id)
                    .Select(x => x.SkillId)
                    .ToListAsync())
                .ToHashSet();

            var currentRequiredSkills = await _context.JobRequiredSkills
                .AsNoTracking()
                .Include(x => x.Skill)
                .Include(x => x.Job)
                .Where(x => x.Job.ApprovalStatus == "APPROVED" && x.Job.JobStatus == "OPEN")
                .ToListAsync();

            var trendingSkills = currentRequiredSkills
                .GroupBy(x => new { x.SkillId, x.Skill.SkillName })
                .Select(group => new TrendingSkillViewModel
                {
                    SkillId = group.Key.SkillId,
                    SkillName = group.Key.SkillName,
                    JobCount = group.Count()
                })
                .OrderByDescending(x => x.JobCount)
                .ThenBy(x => x.SkillName)
                .Take(10)
                .ToList();

            foreach (var skill in trendingSkills)
                skill.AlreadyAdded = mySkillIds.Contains(skill.SkillId);

            var viewModel = new FeedbackHubViewModel
            {
                Resumes = resumes,
                Requests = requests.OrderByDescending(x => x.CreatedAt).ToList(),
                TrendingSkills = trendingSkills
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RequestFeedback(string requestType, string requestMessage, long? resumeId)
        {
            var user = await GetCurrentUserAsync();
            var profileExists = await _context.JobSeekerProfiles.AnyAsync(x => x.JobSeekerId == user.Id);

            if (!profileExists)
            {
                TempData["ErrorMessage"] = "Create your job seeker profile before requesting feedback.";
                return RedirectToAction("Edit", "Profile");
            }

            requestType = requestType?.Trim().ToLowerInvariant() ?? string.Empty;
            requestMessage = requestMessage?.Trim() ?? string.Empty;

            if (requestMessage.Length < 10)
            {
                TempData["ErrorMessage"] = "Tell the counsellor what you would like help with.";
                return RedirectToAction(nameof(Index));
            }

            if (requestMessage.Length > 2000)
            {
                TempData["ErrorMessage"] = "Your request must be 2,000 characters or fewer.";
                return RedirectToAction(nameof(Index));
            }

            var now = DateTime.UtcNow;

            switch (requestType)
            {
                case "resume":
                {
                    if (!resumeId.HasValue || !await _context.Resumes.AnyAsync(x => x.ResumeId == resumeId && x.JobSeekerId == user.Id))
                    {
                        TempData["ErrorMessage"] = "Choose one of your resumes for review.";
                        return RedirectToAction(nameof(Index));
                    }

                    var activeExists = await _context.ResumeFeedback.AnyAsync(x =>
                        x.ResumeId == resumeId.Value &&
                        (x.FeedbackStatus == "NEW" || x.FeedbackStatus == "IN_PROGRESS"));

                    if (activeExists)
                    {
                        TempData["InfoMessage"] = "That resume already has an active feedback request.";
                        return RedirectToAction(nameof(Index));
                    }

                    _context.ResumeFeedback.Add(new ResumeFeedback
                    {
                        ResumeId = resumeId.Value,
                        RequestMessage = requestMessage,
                        FeedbackStatus = "NEW",
                        CreatedAt = now,
                        UpdatedAt = now
                    });
                    break;
                }
                case "career":
                {
                    if (await HasActiveRecommendationAsync(_context.CareerRecommendations.Where(x => x.JobSeekerId == user.Id).Select(x => x.RecommendationStatus)))
                    {
                        TempData["InfoMessage"] = "You already have an active career advice request.";
                        return RedirectToAction(nameof(Index));
                    }

                    _context.CareerRecommendations.Add(new CareerRecommendation
                    {
                        JobSeekerId = user.Id,
                        RequestMessage = requestMessage,
                        RecommendedJobTitle = string.Empty,
                        RecommendationReason = string.Empty,
                        RecommendationSource = "COUNSELLOR",
                        RecommendationStatus = "NEW",
                        CreatedAt = now,
                        UpdatedAt = now
                    });
                    break;
                }
                case "skill":
                {
                    if (await HasActiveRecommendationAsync(_context.SkillRecommendations.Where(x => x.JobSeekerId == user.Id).Select(x => x.RecommendationStatus)))
                    {
                        TempData["InfoMessage"] = "You already have an active skill recommendation request.";
                        return RedirectToAction(nameof(Index));
                    }

                    _context.SkillRecommendations.Add(new SkillRecommendation
                    {
                        JobSeekerId = user.Id,
                        RequestMessage = requestMessage,
                        RecommendationSource = "COUNSELLOR",
                        RecommendationStatus = "NEW",
                        PriorityLevel = "MEDIUM",
                        CreatedAt = now,
                        UpdatedAt = now
                    });
                    break;
                }
                case "certification":
                {
                    if (await HasActiveRecommendationAsync(_context.CertificationRecommendations.Where(x => x.JobSeekerId == user.Id).Select(x => x.RecommendationStatus)))
                    {
                        TempData["InfoMessage"] = "You already have an active certification recommendation request.";
                        return RedirectToAction(nameof(Index));
                    }

                    _context.CertificationRecommendations.Add(new CertificationRecommendation
                    {
                        JobSeekerId = user.Id,
                        RequestMessage = requestMessage,
                        CertificationName = string.Empty,
                        RecommendationSource = "COUNSELLOR",
                        RecommendationStatus = "NEW",
                        PriorityLevel = "MEDIUM",
                        CreatedAt = now,
                        UpdatedAt = now
                    });
                    break;
                }
                default:
                    TempData["ErrorMessage"] = "Choose a valid feedback type.";
                    return RedirectToAction(nameof(Index));
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Your request was sent to the career counsellor.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddTrendingSkill(long skillId)
        {
            var user = await GetCurrentUserAsync();

            var profileExists = await _context.JobSeekerProfiles.AnyAsync(x => x.JobSeekerId == user.Id);
            if (!profileExists)
            {
                TempData["ErrorMessage"] = "Create your job seeker profile before adding skills.";
                return RedirectToAction("Edit", "Profile");
            }

            var skillExists = await _context.Skills.AnyAsync(x => x.SkillId == skillId);
            if (!skillExists)
                return NotFound();

            var alreadyAdded = await _context.JobSeekerSkills.AnyAsync(x =>
                x.JobSeekerId == user.Id && x.SkillId == skillId);

            if (!alreadyAdded)
            {
                _context.JobSeekerSkills.Add(new JobSeekerSkill
                {
                    JobSeekerId = user.Id,
                    SkillId = skillId,
                    ProficiencyLevel = null,
                    CreatedAt = DateTime.UtcNow
                });
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Skill added to your profile.";
            }

            return RedirectToAction(nameof(Index));
        }

        private static Task<bool> HasActiveRecommendationAsync(IQueryable<string> statuses)
        {
            return statuses.AnyAsync(status => status == "NEW" || status == "IN_PROGRESS");
        }

        private async Task<ApplicationUser> GetCurrentUserAsync()
        {
            return await _userManager.GetUserAsync(User)
                ?? throw new InvalidOperationException("The current user could not be loaded.");
        }

        private static string? JoinResponseParts(params (string Label, string? Value)[] parts)
        {
            var values = parts
                .Where(x => !string.IsNullOrWhiteSpace(x.Value))
                .Select(x => $"{x.Label}: {x.Value!.Trim()}")
                .ToList();

            return values.Count == 0 ? null : string.Join("\n", values);
        }

        private static string Friendly(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return string.Empty;
            var words = value.Replace("_", " ").ToLowerInvariant();
            return System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(words);
        }
    }
}
