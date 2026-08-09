using JobSeeker.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobSeeker.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260809000100_AddProfileImageS3Key")]
    public partial class AddProfileImageS3Key : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF NOT EXISTS (
                    SELECT 1 FROM sys.columns
                    WHERE object_id = OBJECT_ID(N'AspNetUsers')
                    AND name = N'ProfileImageS3Key')
                BEGIN
                    ALTER TABLE AspNetUsers
                    ADD ProfileImageS3Key nvarchar(1024) NULL;
                END");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProfileImageS3Key",
                table: "AspNetUsers");
        }
    }
}
