using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobSeeker.Migrations
{
    public partial class AlterJobsTable_AddApprovalColumns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Guard with IF NOT EXISTS so safe to run even if manually added
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'jobs') AND name = N'RejectionReason')
                    ALTER TABLE jobs ADD RejectionReason nvarchar(max) NULL;");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'jobs') AND name = N'ApprovedBy')
                    ALTER TABLE jobs ADD ApprovedBy nvarchar(450) NULL;");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'jobs') AND name = N'ApprovedAt')
                    ALTER TABLE jobs ADD ApprovedAt datetime2 NULL;");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'jobs') AND name = N'IX_jobs_ApprovedBy')
                BEGIN
                    CREATE INDEX IX_jobs_ApprovedBy ON jobs(ApprovedBy);
                END");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex("IX_jobs_ApprovedBy", "jobs");
            migrationBuilder.DropColumn("RejectionReason", "jobs");
            migrationBuilder.DropColumn("ApprovedBy", "jobs");
            migrationBuilder.DropColumn("ApprovedAt", "jobs");
        }
    }
}
