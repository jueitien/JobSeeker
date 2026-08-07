using JobSeeker.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobSeeker.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260807180000_AddJobsAndApplications")]
    public partial class AddJobsAndApplications : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "jobs",
                columns: table => new
                {
                    JobId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployerId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    CompanyName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    JobTitle = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    JobDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Responsibilities = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MinimumQualification = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    PreferredFieldOfStudy = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    MinimumExperienceYears = table.Column<decimal>(type: "decimal(4,1)", precision: 4, scale: 1, nullable: false),
                    EmploymentType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    WorkplaceType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Location = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    MinimumSalary = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: true),
                    MaximumSalary = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: true),
                    VacancyCount = table.Column<int>(type: "int", nullable: false),
                    ApplicationDeadline = table.Column<DateOnly>(type: "date", nullable: true),
                    ApprovalStatus = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false, defaultValue: "APPROVED"),
                    JobStatus = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false, defaultValue: "OPEN"),
                    IsTestData = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_jobs", x => x.JobId);
                    table.ForeignKey(
                        name: "FK_jobs_AspNetUsers_EmployerId",
                        column: x => x.EmployerId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "job_required_skills",
                columns: table => new
                {
                    JobRequiredSkillId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    JobId = table.Column<long>(type: "bigint", nullable: false),
                    SkillId = table.Column<long>(type: "bigint", nullable: false),
                    RequirementType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ImportanceWeight = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_job_required_skills", x => x.JobRequiredSkillId);
                    table.ForeignKey(
                        name: "FK_job_required_skills_jobs_JobId",
                        column: x => x.JobId,
                        principalTable: "jobs",
                        principalColumn: "JobId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_job_required_skills_skills_SkillId",
                        column: x => x.SkillId,
                        principalTable: "skills",
                        principalColumn: "SkillId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "job_applications",
                columns: table => new
                {
                    ApplicationId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    JobId = table.Column<long>(type: "bigint", nullable: false),
                    JobSeekerId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    ResumeId = table.Column<long>(type: "bigint", nullable: false),
                    CoverLetter = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MatchPercentageAtApplication = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    ApplicationStatus = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false, defaultValue: "SUBMITTED"),
                    EmployerNotes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AppliedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_job_applications", x => x.ApplicationId);
                    table.ForeignKey(
                        name: "FK_job_applications_job_seeker_profiles_JobSeekerId",
                        column: x => x.JobSeekerId,
                        principalTable: "job_seeker_profiles",
                        principalColumn: "JobSeekerId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_job_applications_jobs_JobId",
                        column: x => x.JobId,
                        principalTable: "jobs",
                        principalColumn: "JobId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_job_applications_resumes_ResumeId",
                        column: x => x.ResumeId,
                        principalTable: "resumes",
                        principalColumn: "ResumeId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(name: "IX_jobs_EmployerId", table: "jobs", column: "EmployerId");
            migrationBuilder.CreateIndex(name: "IX_jobs_JobStatus_ApprovalStatus", table: "jobs", columns: new[] { "JobStatus", "ApprovalStatus" });
            migrationBuilder.CreateIndex(name: "IX_jobs_Location", table: "jobs", column: "Location");
            migrationBuilder.CreateIndex(name: "IX_job_required_skills_SkillId", table: "job_required_skills", column: "SkillId");
            migrationBuilder.CreateIndex(name: "IX_job_required_skills_JobId_SkillId", table: "job_required_skills", columns: new[] { "JobId", "SkillId" }, unique: true);
            migrationBuilder.CreateIndex(name: "IX_job_applications_ResumeId", table: "job_applications", column: "ResumeId");
            migrationBuilder.CreateIndex(name: "IX_job_applications_JobId_JobSeekerId", table: "job_applications", columns: new[] { "JobId", "JobSeekerId" }, unique: true);
            migrationBuilder.CreateIndex(name: "IX_job_applications_JobSeekerId_ApplicationStatus", table: "job_applications", columns: new[] { "JobSeekerId", "ApplicationStatus" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "job_applications");
            migrationBuilder.DropTable(name: "job_required_skills");
            migrationBuilder.DropTable(name: "jobs");
        }
    }
}
