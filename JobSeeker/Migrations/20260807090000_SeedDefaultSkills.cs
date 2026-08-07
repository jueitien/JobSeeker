using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobSeeker.Migrations
{
    /// <inheritdoc />
    public partial class SeedDefaultSkills : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM [skills] WHERE [SkillName] = N'AWS')
BEGIN
    INSERT INTO [skills] ([SkillName], [CreatedAt])
    VALUES (N'AWS', SYSUTCDATETIME());
END");

            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM [skills] WHERE [SkillName] = N'Microsoft Azure')
BEGIN
    INSERT INTO [skills] ([SkillName], [CreatedAt])
    VALUES (N'Microsoft Azure', SYSUTCDATETIME());
END");

            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM [skills] WHERE [SkillName] = N'C#')
BEGIN
    INSERT INTO [skills] ([SkillName], [CreatedAt])
    VALUES (N'C#', SYSUTCDATETIME());
END");

            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM [skills] WHERE [SkillName] = N'.NET')
BEGIN
    INSERT INTO [skills] ([SkillName], [CreatedAt])
    VALUES (N'.NET', SYSUTCDATETIME());
END");

            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM [skills] WHERE [SkillName] = N'ASP.NET Core')
BEGIN
    INSERT INTO [skills] ([SkillName], [CreatedAt])
    VALUES (N'ASP.NET Core', SYSUTCDATETIME());
END");

            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM [skills] WHERE [SkillName] = N'Java')
BEGIN
    INSERT INTO [skills] ([SkillName], [CreatedAt])
    VALUES (N'Java', SYSUTCDATETIME());
END");

            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM [skills] WHERE [SkillName] = N'Python')
BEGIN
    INSERT INTO [skills] ([SkillName], [CreatedAt])
    VALUES (N'Python', SYSUTCDATETIME());
END");

            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM [skills] WHERE [SkillName] = N'JavaScript')
BEGIN
    INSERT INTO [skills] ([SkillName], [CreatedAt])
    VALUES (N'JavaScript', SYSUTCDATETIME());
END");

            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM [skills] WHERE [SkillName] = N'TypeScript')
BEGIN
    INSERT INTO [skills] ([SkillName], [CreatedAt])
    VALUES (N'TypeScript', SYSUTCDATETIME());
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
IF NOT EXISTS (SELECT 1 FROM [skills] WHERE [SkillName] = N'Bootstrap')
BEGIN
    INSERT INTO [skills] ([SkillName], [CreatedAt])
    VALUES (N'Bootstrap', SYSUTCDATETIME());
END");

            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM [skills] WHERE [SkillName] = N'React')
BEGIN
    INSERT INTO [skills] ([SkillName], [CreatedAt])
    VALUES (N'React', SYSUTCDATETIME());
END");

            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM [skills] WHERE [SkillName] = N'Angular')
BEGIN
    INSERT INTO [skills] ([SkillName], [CreatedAt])
    VALUES (N'Angular', SYSUTCDATETIME());
END");

            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM [skills] WHERE [SkillName] = N'Vue.js')
BEGIN
    INSERT INTO [skills] ([SkillName], [CreatedAt])
    VALUES (N'Vue.js', SYSUTCDATETIME());
END");

            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM [skills] WHERE [SkillName] = N'SQL')
BEGIN
    INSERT INTO [skills] ([SkillName], [CreatedAt])
    VALUES (N'SQL', SYSUTCDATETIME());
END");

            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM [skills] WHERE [SkillName] = N'MySQL')
BEGIN
    INSERT INTO [skills] ([SkillName], [CreatedAt])
    VALUES (N'MySQL', SYSUTCDATETIME());
END");

            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM [skills] WHERE [SkillName] = N'PostgreSQL')
BEGIN
    INSERT INTO [skills] ([SkillName], [CreatedAt])
    VALUES (N'PostgreSQL', SYSUTCDATETIME());
END");

            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM [skills] WHERE [SkillName] = N'SQL Server')
BEGIN
    INSERT INTO [skills] ([SkillName], [CreatedAt])
    VALUES (N'SQL Server', SYSUTCDATETIME());
END");

            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM [skills] WHERE [SkillName] = N'MongoDB')
BEGIN
    INSERT INTO [skills] ([SkillName], [CreatedAt])
    VALUES (N'MongoDB', SYSUTCDATETIME());
END");

            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM [skills] WHERE [SkillName] = N'Git')
BEGIN
    INSERT INTO [skills] ([SkillName], [CreatedAt])
    VALUES (N'Git', SYSUTCDATETIME());
END");

            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM [skills] WHERE [SkillName] = N'GitHub')
BEGIN
    INSERT INTO [skills] ([SkillName], [CreatedAt])
    VALUES (N'GitHub', SYSUTCDATETIME());
END");

            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM [skills] WHERE [SkillName] = N'Docker')
BEGIN
    INSERT INTO [skills] ([SkillName], [CreatedAt])
    VALUES (N'Docker', SYSUTCDATETIME());
END");

            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM [skills] WHERE [SkillName] = N'Kubernetes')
BEGIN
    INSERT INTO [skills] ([SkillName], [CreatedAt])
    VALUES (N'Kubernetes', SYSUTCDATETIME());
END");

            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM [skills] WHERE [SkillName] = N'REST API')
BEGIN
    INSERT INTO [skills] ([SkillName], [CreatedAt])
    VALUES (N'REST API', SYSUTCDATETIME());
END");

            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM [skills] WHERE [SkillName] = N'Entity Framework Core')
BEGIN
    INSERT INTO [skills] ([SkillName], [CreatedAt])
    VALUES (N'Entity Framework Core', SYSUTCDATETIME());
END");

            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM [skills] WHERE [SkillName] = N'Machine Learning')
BEGIN
    INSERT INTO [skills] ([SkillName], [CreatedAt])
    VALUES (N'Machine Learning', SYSUTCDATETIME());
END");

            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM [skills] WHERE [SkillName] = N'Data Analysis')
BEGIN
    INSERT INTO [skills] ([SkillName], [CreatedAt])
    VALUES (N'Data Analysis', SYSUTCDATETIME());
END");

            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM [skills] WHERE [SkillName] = N'Microsoft Excel')
BEGIN
    INSERT INTO [skills] ([SkillName], [CreatedAt])
    VALUES (N'Microsoft Excel', SYSUTCDATETIME());
END");

            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM [skills] WHERE [SkillName] = N'Power BI')
BEGIN
    INSERT INTO [skills] ([SkillName], [CreatedAt])
    VALUES (N'Power BI', SYSUTCDATETIME());
END");

            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM [skills] WHERE [SkillName] = N'Communication')
BEGIN
    INSERT INTO [skills] ([SkillName], [CreatedAt])
    VALUES (N'Communication', SYSUTCDATETIME());
END");

            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM [skills] WHERE [SkillName] = N'Teamwork')
BEGIN
    INSERT INTO [skills] ([SkillName], [CreatedAt])
    VALUES (N'Teamwork', SYSUTCDATETIME());
END");

            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM [skills] WHERE [SkillName] = N'Problem Solving')
BEGIN
    INSERT INTO [skills] ([SkillName], [CreatedAt])
    VALUES (N'Problem Solving', SYSUTCDATETIME());
END");

            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM [skills] WHERE [SkillName] = N'Project Management')
BEGIN
    INSERT INTO [skills] ([SkillName], [CreatedAt])
    VALUES (N'Project Management', SYSUTCDATETIME());
END");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Intentionally keep skills on rollback because users may already be linked to them.
        }
    }
}
