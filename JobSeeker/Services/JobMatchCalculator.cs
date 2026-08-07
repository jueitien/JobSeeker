using JobSeeker.Models;

namespace JobSeeker.Services
{
    public record JobMatchResult(
        decimal Percentage,
        int MatchedSkillCount,
        int RequiredSkillCount,
        List<string> MatchedSkills,
        List<string> MissingSkills);

    public static class JobMatchCalculator
    {
        public static JobMatchResult Calculate(
            JobSeekerProfile? profile,
            HashSet<long> jobSeekerSkillIds,
            Job job)
        {
            var requiredSkills = job.RequiredSkills.ToList();
            var totalWeight = requiredSkills.Sum(x => x.ImportanceWeight);
            decimal matchedWeight = 0;
            var matchedSkills = new List<string>();
            var missingSkills = new List<string>();

            foreach (var required in requiredSkills)
            {
                if (jobSeekerSkillIds.Contains(required.SkillId))
                {
                    matchedWeight += required.ImportanceWeight;
                    matchedSkills.Add(required.Skill.SkillName);
                }
                else
                {
                    missingSkills.Add(required.Skill.SkillName);
                }
            }

            var skillScore = totalWeight > 0
                ? matchedWeight / totalWeight * 100m
                : 0m;

            // Keep the algorithm easy to explain for the project:
            // 70% skills + 10% preferred title + 10% location + 10% study/qualification.
            decimal titleScore = TextMatches(profile?.PreferredJobTitle, job.JobTitle) ? 100m : 0m;
            decimal locationScore = TextMatches(profile?.PreferredLocation, job.Location) ? 100m : 0m;
            decimal educationScore = EducationMatches(profile, job) ? 100m : 0m;

            var overall =
                (skillScore * 0.70m) +
                (titleScore * 0.10m) +
                (locationScore * 0.10m) +
                (educationScore * 0.10m);

            overall = Math.Round(Math.Clamp(overall, 0m, 100m), 2);

            return new JobMatchResult(
                overall,
                matchedSkills.Count,
                requiredSkills.Count,
                matchedSkills,
                missingSkills);
        }

        private static bool TextMatches(string? preferred, string? actual)
        {
            if (string.IsNullOrWhiteSpace(preferred) || string.IsNullOrWhiteSpace(actual))
                return false;

            var left = preferred.Trim();
            var right = actual.Trim();

            if (right.Contains(left, StringComparison.OrdinalIgnoreCase) ||
                left.Contains(right, StringComparison.OrdinalIgnoreCase))
                return true;

            var preferredWords = left.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Where(x => x.Length >= 3)
                .ToArray();

            return preferredWords.Any(word => right.Contains(word, StringComparison.OrdinalIgnoreCase));
        }

        private static bool EducationMatches(JobSeekerProfile? profile, Job job)
        {
            if (profile == null)
                return false;

            if (string.IsNullOrWhiteSpace(job.MinimumQualification) &&
                string.IsNullOrWhiteSpace(job.PreferredFieldOfStudy))
                return true;

            var qualificationMatch = TextMatches(profile.HighestQualification, job.MinimumQualification);
            var fieldMatch = TextMatches(profile.FieldOfStudy, job.PreferredFieldOfStudy);

            return qualificationMatch || fieldMatch;
        }
    }
}
