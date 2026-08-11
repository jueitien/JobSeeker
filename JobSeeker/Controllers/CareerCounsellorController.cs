using System.Security.Claims;
using JobSeeker.Data;
using JobSeeker.Models;
using JobSeeker.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JobSeeker.Controllers;

[Authorize(Roles = UserRoles.CareerCounsellor)]
public class CareerCounsellorController(
    ApplicationDbContext db,
    S3StorageService s3Storage,
    ILogger<CareerCounsellorController> logger) : Controller
{
    [HttpGet]
    public async Task<IActionResult> ResumeReview() => View(await db.ResumeFeedback
        .Include(x => x.Resume).ThenInclude(x => x.JobSeekerProfile).ThenInclude(x => x.User)
        .AsNoTracking().OrderByDescending(x => x.CreatedAt).ToListAsync());

    [HttpGet]
    public async Task<IActionResult> ResumeFeedbackForm(long requestId)
    {
        var record = await db.ResumeFeedback.Include(x => x.Resume)
            .ThenInclude(x => x.JobSeekerProfile).ThenInclude(x => x.User)
            .FirstOrDefaultAsync(x => x.ResumeFeedbackId == requestId);
        return record is null ? NotFound() : View(record);
    }

    [HttpGet]
    public async Task<IActionResult> ViewResume(long requestId)
    {
        var feedback = await db.ResumeFeedback
            .AsNoTracking()
            .Include(record => record.Resume)
            .FirstOrDefaultAsync(record => record.ResumeFeedbackId == requestId);

        if (feedback?.Resume is null ||
            string.IsNullOrWhiteSpace(feedback.Resume.ResumeS3Key))
        {
            return NotFound();
        }

        try
        {
            var presignedUrl = await s3Storage.GetPresignedUrlAsync(
                feedback.Resume.ResumeS3Key,
                TimeSpan.FromMinutes(5));

            return Redirect(presignedUrl);
        }
        catch (Exception exception)
        {
            logger.LogError(
                exception,
                "Failed to create a presigned S3 URL for resume {Key}.",
                feedback.Resume.ResumeS3Key);

            TempData["ErrorMessage"] =
                "The resume could not be opened. Check the S3 connection and try again.";

            return RedirectToAction(
                nameof(ResumeFeedbackForm),
                new { requestId });
        }
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveResumeFeedback(ResumeFeedback input, string command)
    {
        var record = await db.ResumeFeedback.FindAsync(input.ResumeFeedbackId);
        if (record is null) return NotFound();
        record.OverallComment = input.OverallComment;
        record.Strengths = input.Strengths;
        record.Weaknesses = input.Weaknesses;
        record.RecommendedChanges = input.RecommendedChanges;
        SetOwnershipAndStatus(record, command);
        await db.SaveChangesAsync();
        return command == "complete" ? RedirectToAction(nameof(ResumeReview)) : RedirectToAction(nameof(ResumeFeedbackForm), new { requestId = record.ResumeFeedbackId });
    }

    [HttpGet]
    public async Task<IActionResult> CareerReview() => View(await db.CareerRecommendations
        .Include(x => x.JobSeeker).ThenInclude(x => x.User).AsNoTracking()
        .OrderByDescending(x => x.CreatedAt).ToListAsync());

    [HttpGet]
    public async Task<IActionResult> CareerRecommendations(long requestId)
    {
        var record = await db.CareerRecommendations.Include(x => x.JobSeeker).ThenInclude(x => x.User)
            .FirstOrDefaultAsync(x => x.CareerRecommendationId == requestId);
        return record is null ? NotFound() : View(record);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveCareerRecommendation(CareerRecommendation input, string command)
    {
        var record = await db.CareerRecommendations.FindAsync(input.CareerRecommendationId);
        if (record is null) return NotFound();
        record.RecommendedJobTitle = input.RecommendedJobTitle;
        record.RecommendedIndustry = input.RecommendedIndustry;
        record.RecommendationReason = input.RecommendationReason;
        record.RequiredImprovements = input.RequiredImprovements;
        SetOwnershipAndStatus(record, command);
        await db.SaveChangesAsync();
        return command == "complete" ? RedirectToAction(nameof(CareerReview)) : RedirectToAction(nameof(CareerRecommendations), new { requestId = record.CareerRecommendationId });
    }

    [HttpGet]
    public async Task<IActionResult> SkillReview() => View(await db.SkillRecommendations
        .Include(x => x.JobSeeker).ThenInclude(x => x.User).AsNoTracking()
        .OrderByDescending(x => x.CreatedAt).ToListAsync());

    [HttpGet]
    public async Task<IActionResult> SkillRecommendations(long requestId)
    {
        var record = await db.SkillRecommendations.Include(x => x.JobSeeker).ThenInclude(x => x.User)
            .FirstOrDefaultAsync(x => x.SkillRecommendationId == requestId);
        return record is null ? NotFound() : View(record);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveSkillRecommendation(SkillRecommendation input, string command)
    {
        var record = await db.SkillRecommendations.FindAsync(input.SkillRecommendationId);
        if (record is null) return NotFound();
        record.RecommendedSkill = input.RecommendedSkill;
        record.PriorityLevel = input.PriorityLevel;
        record.RecommendationReason = input.RecommendationReason;
        SetOwnershipAndStatus(record, command);
        await db.SaveChangesAsync();
        return command == "complete" ? RedirectToAction(nameof(SkillReview)) : RedirectToAction(nameof(SkillRecommendations), new { requestId = record.SkillRecommendationId });
    }

    [HttpGet]
    public async Task<IActionResult> CertificationReview() => View(await db.CertificationRecommendations
        .Include(x => x.JobSeeker).ThenInclude(x => x.User).AsNoTracking()
        .OrderByDescending(x => x.CreatedAt).ToListAsync());

    [HttpGet]
    public async Task<IActionResult> CertificationRecommendations(long requestId)
    {
        var record = await db.CertificationRecommendations.Include(x => x.JobSeeker).ThenInclude(x => x.User)
            .FirstOrDefaultAsync(x => x.CertificationRecommendationId == requestId);
        return record is null ? NotFound() : View(record);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveCertificationRecommendation(CertificationRecommendation input, string command)
    {
        var record = await db.CertificationRecommendations.FindAsync(input.CertificationRecommendationId);
        if (record is null) return NotFound();
        record.CertificationName = input.CertificationName;
        record.IssuingOrganization = input.IssuingOrganization;
        record.PriorityLevel = input.PriorityLevel;
        record.RecommendationReason = input.RecommendationReason;
        SetOwnershipAndStatus(record, command);
        await db.SaveChangesAsync();
        return command == "complete" ? RedirectToAction(nameof(CertificationReview)) : RedirectToAction(nameof(CertificationRecommendations), new { requestId = record.CertificationRecommendationId });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(string recordType, long id, string status)
    {
        if (status is not ("IN_PROGRESS" or "DISMISSED")) return BadRequest();
        switch (recordType)
        {
            case "resume":
                var resume = await db.ResumeFeedback.FindAsync(id); if (resume is null) return NotFound(); SetOwnershipAndStatus(resume, status); break;
            case "career":
                var career = await db.CareerRecommendations.FindAsync(id); if (career is null) return NotFound(); SetOwnershipAndStatus(career, status); break;
            case "skill":
                var skill = await db.SkillRecommendations.FindAsync(id); if (skill is null) return NotFound(); SetOwnershipAndStatus(skill, status); break;
            case "certification":
                var certification = await db.CertificationRecommendations.FindAsync(id); if (certification is null) return NotFound(); SetOwnershipAndStatus(certification, status); break;
            default: return BadRequest();
        }
        await db.SaveChangesAsync();
        return RedirectToAction(recordType switch { "resume" => nameof(ResumeReview), "career" => nameof(CareerReview), "skill" => nameof(SkillReview), _ => nameof(CertificationReview) });
    }

    private string CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;
    private void SetOwnershipAndStatus(ResumeFeedback record, string command) { record.CounsellorId = CurrentUserId; record.FeedbackStatus = Status(command); record.UpdatedAt = DateTime.UtcNow; }
    private void SetOwnershipAndStatus(CareerRecommendation record, string command) { record.CounsellorId = CurrentUserId; record.RecommendationStatus = Status(command); record.RecommendationSource = "COUNSELLOR"; record.UpdatedAt = DateTime.UtcNow; }
    private void SetOwnershipAndStatus(SkillRecommendation record, string command) { record.CounsellorId = CurrentUserId; record.RecommendationStatus = Status(command); record.RecommendationSource = "COUNSELLOR"; record.UpdatedAt = DateTime.UtcNow; }
    private void SetOwnershipAndStatus(CertificationRecommendation record, string command) { record.CounsellorId = CurrentUserId; record.RecommendationStatus = Status(command); record.RecommendationSource = "COUNSELLOR"; record.UpdatedAt = DateTime.UtcNow; }
    private static string Status(string command) => command switch { "complete" => "COMPLETED", "DISMISSED" => "DISMISSED", _ => "IN_PROGRESS" };
}
