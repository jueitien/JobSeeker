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
            migrationBuilder.AddColumn<string>(
                name: "ProfileImageS3Key",
                table: "AspNetUsers",
                type: "nvarchar(1024)",
                maxLength: 1024,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProfileImageS3Key",
                table: "AspNetUsers");
        }
    }
}
