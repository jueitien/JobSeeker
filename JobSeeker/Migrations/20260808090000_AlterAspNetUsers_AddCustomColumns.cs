using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobSeeker.Migrations
{
    /// <summary>
    /// Adds custom columns (AccountStatus, ProfileImageS3Key, LastLoginAt, CreatedAt, UpdatedAt)
    /// to AspNetUsers that were originally added by Jacky's branch in the Temp migration folder
    /// but were never merged into the main migration chain.
    /// </summary>
    public partial class AlterAspNetUsers_AddCustomColumns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Guard each column with IF NOT EXISTS so this is safe to run
            // even if the column was already added manually via SQL.

            migrationBuilder.Sql(@"
                IF NOT EXISTS (
                    SELECT 1 FROM sys.columns
                    WHERE object_id = OBJECT_ID(N'AspNetUsers')
                    AND name = N'AccountStatus')
                BEGIN
                    ALTER TABLE AspNetUsers
                    ADD AccountStatus nvarchar(30) NOT NULL DEFAULT 'ACTIVE';
                END");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (
                    SELECT 1 FROM sys.columns
                    WHERE object_id = OBJECT_ID(N'AspNetUsers')
                    AND name = N'ProfileImageS3Key')
                BEGIN
                    ALTER TABLE AspNetUsers
                    ADD ProfileImageS3Key nvarchar(1024) NULL;
                END");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (
                    SELECT 1 FROM sys.columns
                    WHERE object_id = OBJECT_ID(N'AspNetUsers')
                    AND name = N'LastLoginAt')
                BEGIN
                    ALTER TABLE AspNetUsers
                    ADD LastLoginAt datetime2 NULL;
                END");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (
                    SELECT 1 FROM sys.columns
                    WHERE object_id = OBJECT_ID(N'AspNetUsers')
                    AND name = N'CreatedAt')
                BEGIN
                    ALTER TABLE AspNetUsers
                    ADD CreatedAt datetime2 NOT NULL DEFAULT GETUTCDATE();
                END");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (
                    SELECT 1 FROM sys.columns
                    WHERE object_id = OBJECT_ID(N'AspNetUsers')
                    AND name = N'UpdatedAt')
                BEGIN
                    ALTER TABLE AspNetUsers
                    ADD UpdatedAt datetime2 NOT NULL DEFAULT GETUTCDATE();
                END");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "AccountStatus",    table: "AspNetUsers");
            migrationBuilder.DropColumn(name: "ProfileImageS3Key", table: "AspNetUsers");
            migrationBuilder.DropColumn(name: "LastLoginAt",      table: "AspNetUsers");
            migrationBuilder.DropColumn(name: "CreatedAt",        table: "AspNetUsers");
            migrationBuilder.DropColumn(name: "UpdatedAt",        table: "AspNetUsers");
        }
    }
}
