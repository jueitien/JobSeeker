using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobSeeker.Migrations
{
    public partial class CreateAuditLogs : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "audit_logs",
                columns: table => new
                {
                    AuditLogId        = table.Column<long>(type: "bigint", nullable: false)
                                            .Annotation("SqlServer:Identity", "1, 1"),
                    UserId            = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    ActionType        = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EntityType        = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    EntityId          = table.Column<long>(type: "bigint", nullable: true),
                    ActionDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IpAddress         = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true),
                    CreatedAt         = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_audit_logs", x => x.AuditLogId);
                    table.ForeignKey("FK_audit_logs_AspNetUsers_UserId", x => x.UserId, "AspNetUsers", "Id", onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex("IX_audit_logs_UserId",    "audit_logs", "UserId");
            migrationBuilder.CreateIndex("IX_audit_logs_CreatedAt", "audit_logs", "CreatedAt");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable("audit_logs");
        }
    }
}
