using JobSeeker.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace JobSeeker.Data;

public static class CareerCounsellorSeeder
{
    public static async Task SeedRequestsAsync(IServiceProvider services)
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

        if (!await context.JobSeekerProfiles.AnyAsync())
        {
            await SeedJobSeekersAsync(context, userManager);
        }

        var profiles = await context.JobSeekerProfiles
            .AsNoTracking()
            .OrderBy(profile => profile.CreatedAt)
            .Take(4)
            .ToListAsync();

        if (profiles.Count == 0)
        {
            return;
        }

        var now = DateTime.UtcNow;

        if (!await context.CareerRecommendations.AnyAsync())
        {
            context.CareerRecommendations.AddRange(profiles.Select((profile, index) =>
                new CareerRecommendation
                {
                    JobSeekerId = profile.JobSeekerId,
                    RequestMessage = "Please recommend a suitable career based on my profile, skills and interests.",
                    RecommendedJobTitle = index == 1 ? "Business Analyst" : "Pending counsellor review",
                    RecommendedIndustry = index == 1 ? "Information Technology" : null,
                    RecommendationReason = index == 1
                        ? "The role aligns with the job seeker's analytical and communication background."
                        : "To be completed by a career counsellor.",
                    RequiredImprovements = index == 1
                        ? "Strengthen SQL, data visualisation and requirements-analysis skills."
                        : null,
                    RecommendationStatus = StatusFor(index),
                    CreatedAt = now.AddDays(-index),
                    UpdatedAt = now.AddDays(-index)
                }));
        }

        if (!await context.SkillRecommendations.AnyAsync())
        {
            context.SkillRecommendations.AddRange(profiles.Select((profile, index) =>
                new SkillRecommendation
                {
                    JobSeekerId = profile.JobSeekerId,
                    RequestMessage = "Please identify skills that would improve my employment opportunities.",
                    RecommendedSkill = index == 1 ? "SQL and data visualisation" : null,
                    RecommendationReason = index == 1
                        ? "These skills support the job seeker's stated career direction."
                        : null,
                    PriorityLevel = index == 1 ? "HIGH" : "MEDIUM",
                    RecommendationStatus = StatusFor(index),
                    CreatedAt = now.AddDays(-index),
                    UpdatedAt = now.AddDays(-index)
                }));
        }

        if (!await context.CertificationRecommendations.AnyAsync())
        {
            context.CertificationRecommendations.AddRange(profiles.Select((profile, index) =>
                new CertificationRecommendation
                {
                    JobSeekerId = profile.JobSeekerId,
                    RequestMessage = "Please recommend a certification that supports my preferred career path.",
                    CertificationName = index == 1 ? "Microsoft Azure Fundamentals" : "Pending counsellor review",
                    IssuingOrganization = index == 1 ? "Microsoft" : null,
                    RecommendationReason = index == 1
                        ? "It provides a recognised foundation in cloud concepts and services."
                        : null,
                    PriorityLevel = index == 1 ? "HIGH" : "MEDIUM",
                    RecommendationStatus = StatusFor(index),
                    CreatedAt = now.AddDays(-index),
                    UpdatedAt = now.AddDays(-index)
                }));
        }

        if (!await context.ResumeFeedback.AnyAsync())
        {
            var resumes = await context.Resumes
                .AsNoTracking()
                .OrderBy(resume => resume.UploadedAt)
                .Take(4)
                .ToListAsync();

            context.ResumeFeedback.AddRange(resumes.Select((resume, index) =>
                new ResumeFeedback
                {
                    ResumeId = resume.ResumeId,
                    RequestMessage = "Please review my resume and suggest improvements before I apply for jobs.",
                    OverallComment = index == 1
                        ? "The resume has a clear structure but would benefit from more measurable achievements."
                        : null,
                    Strengths = index == 1 ? "Clear education and technical-skills sections." : null,
                    Weaknesses = index == 1 ? "Experience descriptions focus on duties instead of outcomes." : null,
                    RecommendedChanges = index == 1 ? "Add measurable results and tailor the summary to the target role." : null,
                    FeedbackStatus = StatusFor(index),
                    CreatedAt = now.AddDays(-index),
                    UpdatedAt = now.AddDays(-index)
                }));
        }

        await context.SaveChangesAsync();
    }

    private static async Task SeedJobSeekersAsync(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        var people = new[]
        {
            (Name: "Ali Ahmad", Email: "ali.demo@jobseeker.com", Title: "Software Developer Resume", Objective: "Junior software developer"),
            (Name: "Siti Aminah", Email: "siti.demo@jobseeker.com", Title: "Business Analyst Resume", Objective: "Business and data analyst"),
            (Name: "John Tan", Email: "john.demo@jobseeker.com", Title: "UI UX Designer Resume", Objective: "User experience designer"),
            (Name: "Nur Aisyah", Email: "aisyah.demo@jobseeker.com", Title: "Marketing Executive Resume", Objective: "Digital marketing executive")
        };

        foreach (var person in people)
        {
            var user = await userManager.FindByEmailAsync(person.Email);

            if (user is null)
            {
                user = new ApplicationUser
                {
                    FullName = person.Name,
                    UserName = person.Email,
                    Email = person.Email,
                    EmailConfirmed = true,
                    AccountStatus = "ACTIVE",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var result = await userManager.CreateAsync(user, "Demo@12345");
                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(error => error.Description));
                    throw new InvalidOperationException($"Could not create demo job seeker: {errors}");
                }

                await userManager.AddToRoleAsync(user, UserRoles.JobSeeker);
            }

            if (await context.JobSeekerProfiles.AnyAsync(profile => profile.JobSeekerId == user.Id))
            {
                continue;
            }

            context.JobSeekerProfiles.Add(new JobSeekerProfile
            {
                JobSeekerId = user.Id,
                ProfileDescription = $"Demo profile for {person.Name}.",
                CareerObjective = person.Objective,
                HighestQualification = "Bachelor Degree",
                FieldOfStudy = "Information Technology",
                UniversityName = "Demo University",
                GraduationYear = 2025,
                PreferredJobTitle = person.Objective,
                PreferredLocation = "Kota Kinabalu, Sabah",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });

            await context.SaveChangesAsync();

            context.Resumes.Add(new Resume
            {
                JobSeekerId = user.Id,
                ResumeTitle = person.Title,
                ResumeDescription = "Demo resume created for career counsellor workflow testing.",
                ResumeS3Key = $"demo-resumes/{user.Id}.pdf",
                ExtractedText = "Education, skills and work experience for counselling review.",
                IsPrimary = true,
                UploadedAt = DateTime.UtcNow
            });

            await context.SaveChangesAsync();
        }
    }

    private static string StatusFor(int index) => index switch
    {
        1 => "IN_PROGRESS",
        2 => "COMPLETED",
        3 => "DISMISSED",
        _ => "NEW"
    };
}
