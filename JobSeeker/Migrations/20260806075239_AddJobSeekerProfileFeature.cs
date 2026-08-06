using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobSeeker.Migrations
{
    /// <inheritdoc />
    public partial class AddJobSeekerProfileFeature : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "job_seeker_profiles",
                columns: table => new
                {
                    JobSeekerId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    ProfileDescription = table.Column<string>(type: "nvarchar(max)", maxLength: 4000, nullable: true),
                    CareerObjective = table.Column<string>(type: "nvarchar(max)", maxLength: 4000, nullable: true),
                    HighestQualification = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    FieldOfStudy = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    UniversityName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    GraduationYear = table.Column<int>(type: "int", nullable: true),
                    ExperienceDescription = table.Column<string>(type: "nvarchar(max)", maxLength: 4000, nullable: true),
                    PreferredJobTitle = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    PreferredLocation = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    ExpectedSalary = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: true),
                    AvailabilityDate = table.Column<DateOnly>(type: "date", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_job_seeker_profiles", x => x.JobSeekerId);
                    table.ForeignKey(
                        name: "FK_job_seeker_profiles_AspNetUsers_JobSeekerId",
                        column: x => x.JobSeekerId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "skills",
                columns: table => new
                {
                    SkillId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SkillName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_skills", x => x.SkillId);
                });

            migrationBuilder.CreateTable(
                name: "certifications",
                columns: table => new
                {
                    CertificationId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    JobSeekerId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    CertificationName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CertificateS3Key = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_certifications", x => x.CertificationId);
                    table.ForeignKey(
                        name: "FK_certifications_job_seeker_profiles_JobSeekerId",
                        column: x => x.JobSeekerId,
                        principalTable: "job_seeker_profiles",
                        principalColumn: "JobSeekerId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "resumes",
                columns: table => new
                {
                    ResumeId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    JobSeekerId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    ResumeTitle = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ResumeDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ResumeS3Key = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: false),
                    ExtractedText = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsPrimary = table.Column<bool>(type: "bit", nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_resumes", x => x.ResumeId);
                    table.ForeignKey(
                        name: "FK_resumes_job_seeker_profiles_JobSeekerId",
                        column: x => x.JobSeekerId,
                        principalTable: "job_seeker_profiles",
                        principalColumn: "JobSeekerId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "job_seeker_skills",
                columns: table => new
                {
                    JobSeekerSkillId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    JobSeekerId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    SkillId = table.Column<long>(type: "bigint", nullable: false),
                    ProficiencyLevel = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_job_seeker_skills", x => x.JobSeekerSkillId);
                    table.ForeignKey(
                        name: "FK_job_seeker_skills_job_seeker_profiles_JobSeekerId",
                        column: x => x.JobSeekerId,
                        principalTable: "job_seeker_profiles",
                        principalColumn: "JobSeekerId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_job_seeker_skills_skills_SkillId",
                        column: x => x.SkillId,
                        principalTable: "skills",
                        principalColumn: "SkillId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_certifications_JobSeekerId",
                table: "certifications",
                column: "JobSeekerId");

            migrationBuilder.CreateIndex(
                name: "IX_job_seeker_skills_JobSeekerId_SkillId",
                table: "job_seeker_skills",
                columns: new[] { "JobSeekerId", "SkillId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_job_seeker_skills_SkillId",
                table: "job_seeker_skills",
                column: "SkillId");

            migrationBuilder.CreateIndex(
                name: "IX_resumes_JobSeekerId",
                table: "resumes",
                column: "JobSeekerId");

            migrationBuilder.CreateIndex(
                name: "IX_skills_SkillName",
                table: "skills",
                column: "SkillName",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "certifications");

            migrationBuilder.DropTable(
                name: "job_seeker_skills");

            migrationBuilder.DropTable(
                name: "resumes");

            migrationBuilder.DropTable(
                name: "skills");

            migrationBuilder.DropTable(
                name: "job_seeker_profiles");
        }
    }
}
