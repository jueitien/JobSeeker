using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobSeeker.Migrations;

public partial class AddCareerCounsellorTables : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "career_recommendations",
            columns: table => new
            {
                career_recommendation_id = table.Column<long>(type: "bigint", nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                job_seeker_id = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                counsellor_id = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                request_message = table.Column<string>(type: "nvarchar(max)", nullable: true),
                recommended_job_title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                recommended_industry = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                recommendation_reason = table.Column<string>(type: "nvarchar(max)", nullable: false),
                required_improvements = table.Column<string>(type: "nvarchar(max)", nullable: true),
                recommendation_source = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false, defaultValue: "COUNSELLOR"),
                recommendation_status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false, defaultValue: "NEW"),
                created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                updated_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_career_recommendations", x => x.career_recommendation_id);
                table.CheckConstraint("chk_career_recommendation_source", "[recommendation_source] IN ('SYSTEM','COUNSELLOR')");
                table.CheckConstraint("chk_career_recommendation_status", "[recommendation_status] IN ('NEW','IN_PROGRESS','COMPLETED','DISMISSED')");
                table.ForeignKey("FK_career_recommendations_AspNetUsers_counsellor_id", x => x.counsellor_id, "AspNetUsers", "Id");
                table.ForeignKey("FK_career_recommendations_job_seeker_profiles_job_seeker_id", x => x.job_seeker_id, "job_seeker_profiles", "JobSeekerId", onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "certification_recommendations",
            columns: table => new
            {
                certification_recommendation_id = table.Column<long>(type: "bigint", nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                job_seeker_id = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                counsellor_id = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                request_message = table.Column<string>(type: "nvarchar(max)", nullable: true),
                certification_name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                issuing_organization = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                recommendation_reason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                priority_level = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "MEDIUM"),
                recommendation_source = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false, defaultValue: "COUNSELLOR"),
                recommendation_status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false, defaultValue: "NEW"),
                created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                updated_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_certification_recommendations", x => x.certification_recommendation_id);
                table.CheckConstraint("chk_certification_priority", "[priority_level] IN ('LOW','MEDIUM','HIGH')");
                table.CheckConstraint("chk_certification_recommendation_source", "[recommendation_source] IN ('SYSTEM','COUNSELLOR')");
                table.CheckConstraint("chk_certification_recommendation_status", "[recommendation_status] IN ('NEW','IN_PROGRESS','COMPLETED','DISMISSED')");
                table.ForeignKey("FK_certification_recommendations_AspNetUsers_counsellor_id", x => x.counsellor_id, "AspNetUsers", "Id");
                table.ForeignKey("FK_certification_recommendations_job_seeker_profiles_job_seeker_id", x => x.job_seeker_id, "job_seeker_profiles", "JobSeekerId", onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "skill_recommendations",
            columns: table => new
            {
                skill_recommendation_id = table.Column<long>(type: "bigint", nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                job_seeker_id = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                counsellor_id = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                request_message = table.Column<string>(type: "nvarchar(max)", nullable: true),
                recommended_skill = table.Column<string>(type: "nvarchar(max)", nullable: true),
                recommendation_reason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                priority_level = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "MEDIUM"),
                recommendation_source = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false, defaultValue: "COUNSELLOR"),
                recommendation_status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false, defaultValue: "NEW"),
                created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                updated_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_skill_recommendations", x => x.skill_recommendation_id);
                table.CheckConstraint("chk_skill_priority", "[priority_level] IN ('LOW','MEDIUM','HIGH')");
                table.CheckConstraint("chk_skill_recommendation_source", "[recommendation_source] IN ('SYSTEM','COUNSELLOR')");
                table.CheckConstraint("chk_skill_recommendation_status", "[recommendation_status] IN ('NEW','IN_PROGRESS','COMPLETED','DISMISSED')");
                table.ForeignKey("FK_skill_recommendations_AspNetUsers_counsellor_id", x => x.counsellor_id, "AspNetUsers", "Id");
                table.ForeignKey("FK_skill_recommendations_job_seeker_profiles_job_seeker_id", x => x.job_seeker_id, "job_seeker_profiles", "JobSeekerId", onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "resume_feedback",
            columns: table => new
            {
                resume_feedback_id = table.Column<long>(type: "bigint", nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                resume_id = table.Column<long>(type: "bigint", nullable: false),
                counsellor_id = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                overall_comment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                request_message = table.Column<string>(type: "nvarchar(max)", nullable: true),
                strengths = table.Column<string>(type: "nvarchar(max)", nullable: true),
                weaknesses = table.Column<string>(type: "nvarchar(max)", nullable: true),
                recommended_changes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                feedback_status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false, defaultValue: "NEW"),
                created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                updated_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_resume_feedback", x => x.resume_feedback_id);
                table.CheckConstraint("chk_feedback_status", "[feedback_status] IN ('NEW','IN_PROGRESS','COMPLETED','DISMISSED')");
                table.ForeignKey("FK_resume_feedback_AspNetUsers_counsellor_id", x => x.counsellor_id, "AspNetUsers", "Id");
                table.ForeignKey("FK_resume_feedback_resumes_resume_id", x => x.resume_id, "resumes", "ResumeId", onDelete: ReferentialAction.Cascade);
            });

        foreach (var (table, column) in new[]
        {
            ("career_recommendations", "counsellor_id"), ("career_recommendations", "job_seeker_id"),
            ("certification_recommendations", "counsellor_id"), ("certification_recommendations", "job_seeker_id"),
            ("skill_recommendations", "counsellor_id"), ("skill_recommendations", "job_seeker_id"),
            ("resume_feedback", "counsellor_id"), ("resume_feedback", "resume_id")
        }) migrationBuilder.CreateIndex(name: $"IX_{table}_{column}", table: table, column: column);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable("career_recommendations");
        migrationBuilder.DropTable("certification_recommendations");
        migrationBuilder.DropTable("resume_feedback");
        migrationBuilder.DropTable("skill_recommendations");
    }
}
