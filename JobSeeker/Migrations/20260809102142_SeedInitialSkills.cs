using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobSeeker.Migrations
{
    /// <inheritdoc />
    public partial class SeedInitialSkills : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Seed the 5 skills used by the "Skills Requirement" section of the
            // Post a Vacancy form (job_required_skills references these rows).
            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM [skills] WHERE [SkillName] = N'C++')
BEGIN
    INSERT INTO [skills] ([SkillName], [CreatedAt])
    VALUES (N'C++', SYSUTCDATETIME());
END");

            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM [skills] WHERE [SkillName] = N'Java')
BEGIN
    INSERT INTO [skills] ([SkillName], [CreatedAt])
    VALUES (N'Java', SYSUTCDATETIME());
END");

            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM [skills] WHERE [SkillName] = N'HTML')
BEGIN
    INSERT INTO [skills] ([SkillName], [CreatedAt])
    VALUES (N'HTML', SYSUTCDATETIME());
END");

            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM [skills] WHERE [SkillName] = N'CSS')
BEGIN
    INSERT INTO [skills] ([SkillName], [CreatedAt])
    VALUES (N'CSS', SYSUTCDATETIME());
END");

            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM [skills] WHERE [SkillName] = N'JavaScript')
BEGIN
    INSERT INTO [skills] ([SkillName], [CreatedAt])
    VALUES (N'JavaScript', SYSUTCDATETIME());
END");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DELETE FROM [skills] WHERE [SkillName] IN (N'C++', N'Java', N'HTML', N'CSS', N'JavaScript')
AND [SkillId] NOT IN (SELECT DISTINCT [SkillId] FROM [job_required_skills])
AND [SkillId] NOT IN (SELECT DISTINCT [SkillId] FROM [job_seeker_skills]);");
        }
    }
}
