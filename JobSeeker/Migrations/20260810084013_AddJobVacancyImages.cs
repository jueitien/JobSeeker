using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobSeeker.Migrations
{
    /// <inheritdoc />
    public partial class AddJobVacancyImages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // NOTE: This migration was originally scaffolded alongside several
            // spurious operations (renaming job_seeker_profiles columns,
            // re-creating employer_profiles, re-adding jobs.ApprovedAt) caused
            // by drift between the old model snapshot and the already-applied
            // database schema. Those objects already exist in the database
            // from earlier migrations, so only the genuinely new
            // job_vacancy_images table is created here.
            migrationBuilder.CreateTable(
                name: "job_vacancy_images",
                columns: table => new
                {
                    JobVacancyImageId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    JobId = table.Column<long>(type: "bigint", nullable: false),
                    ImageS3Key = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_job_vacancy_images", x => x.JobVacancyImageId);
                    table.ForeignKey(
                        name: "FK_job_vacancy_images_jobs_JobId",
                        column: x => x.JobId,
                        principalTable: "jobs",
                        principalColumn: "JobId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_job_vacancy_images_JobId",
                table: "job_vacancy_images",
                column: "JobId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "job_vacancy_images");
        }
    }
}
