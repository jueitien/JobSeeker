using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobSeeker.Migrations.Temp
{
    /// <inheritdoc />
    public partial class ShowPending : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "career_recommendations");

            migrationBuilder.DropTable(
                name: "certification_recommendations");

            migrationBuilder.DropTable(
                name: "resume_feedback");

            migrationBuilder.DropTable(
                name: "skill_recommendations");

            migrationBuilder.DropCheckConstraint(
                name: "CK_AspNetUsers_AccountStatus",
                table: "AspNetUsers");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "GETDATE()");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "GETDATE()");

            migrationBuilder.AlterColumn<string>(
                name: "AccountStatus",
                table: "AspNetUsers",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30,
                oldDefaultValue: "ACTIVE");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<string>(
                name: "AccountStatus",
                table: "AspNetUsers",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "ACTIVE",
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30);

            migrationBuilder.CreateTable(
                name: "career_recommendations",
                columns: table => new
                {
                    career_recommendation_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    counsellor_id = table.Column<long>(type: "bigint", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    job_seeker_id = table.Column<long>(type: "bigint", nullable: false),
                    recommendation_reason = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    recommendation_source = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    recommendation_status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    recommended_industry = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    recommended_job_title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    request_message = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    required_improvements = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_career_recommendations", x => x.career_recommendation_id);
                });

            migrationBuilder.CreateTable(
                name: "certification_recommendations",
                columns: table => new
                {
                    certification_recommendation_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    certification_name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    counsellor_id = table.Column<long>(type: "bigint", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    issuing_organization = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    job_seeker_id = table.Column<long>(type: "bigint", nullable: false),
                    priority_level = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    recommendation_reason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    recommendation_source = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    recommendation_status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    request_message = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_certification_recommendations", x => x.certification_recommendation_id);
                });

            migrationBuilder.CreateTable(
                name: "resume_feedback",
                columns: table => new
                {
                    resume_feedback_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    counsellor_id = table.Column<long>(type: "bigint", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    feedback_status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    overall_comment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    recommended_changes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    request_message = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    resume_id = table.Column<long>(type: "bigint", nullable: false),
                    strengths = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    weaknesses = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_resume_feedback", x => x.resume_feedback_id);
                });

            migrationBuilder.CreateTable(
                name: "skill_recommendations",
                columns: table => new
                {
                    skill_recommendation_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    counsellor_id = table.Column<long>(type: "bigint", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    job_seeker_id = table.Column<long>(type: "bigint", nullable: false),
                    priority_level = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    recommendation_reason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    recommendation_source = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    recommendation_status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    request_message = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    skill_id = table.Column<long>(type: "bigint", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_skill_recommendations", x => x.skill_recommendation_id);
                });

            migrationBuilder.AddCheckConstraint(
                name: "CK_AspNetUsers_AccountStatus",
                table: "AspNetUsers",
                sql: "[AccountStatus] IN ('PENDING','ACTIVE','SUSPENDED','DEACTIVATED')");
        }
    }
}
