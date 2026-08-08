using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobSeeker.Migrations
{
    public partial class CreateSystemReports : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "system_reports",
                columns: table => new
                {
                    SystemReportId  = table.Column<long>(type: "bigint", nullable: false)
                                          .Annotation("SqlServer:Identity", "1, 1"),
                    GeneratedBy     = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    ReportName      = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ReportType      = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ReportParameters = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OriginalFileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    ReportS3Key     = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    FileContentType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FileSizeBytes   = table.Column<long>(type: "bigint", nullable: true),
                    GeneratedAt     = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_system_reports", x => x.SystemReportId);
                    table.ForeignKey("FK_system_reports_AspNetUsers_GeneratedBy", x => x.GeneratedBy, "AspNetUsers", "Id", onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex("IX_system_reports_GeneratedBy",  "system_reports", "GeneratedBy");
            migrationBuilder.CreateIndex("IX_system_reports_GeneratedAt",  "system_reports", "GeneratedAt");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable("system_reports");
        }
    }
}
