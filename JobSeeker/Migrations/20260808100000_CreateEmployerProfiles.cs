using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobSeeker.Migrations
{
    public partial class CreateEmployerProfiles : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "employer_profiles",
                columns: table => new
                {
                    EmployerId        = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    CompanyName       = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CompanyRegistrationNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Industry          = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    CompanySize       = table.Column<string>(type: "nvarchar(50)",  maxLength: 50,  nullable: true),
                    CompanyDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CompanyWebsite    = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CompanyAddress    = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CompanyLogoS3Key  = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    VerificationStatus = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false, defaultValue: "PENDING"),
                    VerificationRemarks = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VerifiedBy        = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    VerifiedAt        = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt         = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt         = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_employer_profiles", x => x.EmployerId);
                    table.ForeignKey("FK_employer_profiles_AspNetUsers_EmployerId", x => x.EmployerId, "AspNetUsers", "Id", onDelete: ReferentialAction.Cascade);
                    table.ForeignKey("FK_employer_profiles_AspNetUsers_VerifiedBy",  x => x.VerifiedBy, "AspNetUsers", "Id", onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateIndex("IX_employer_profiles_VerifiedBy", "employer_profiles", "VerifiedBy");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable("employer_profiles");
        }
    }
}
