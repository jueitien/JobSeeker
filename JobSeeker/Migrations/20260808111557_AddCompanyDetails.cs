using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobSeeker.Migrations
{
    /// <inheritdoc />
    public partial class AddCompanyDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "company_details",
                columns: table => new
                {
                    CompanyDetailId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployerId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    CompanyName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_company_details", x => x.CompanyDetailId);
                    table.ForeignKey(
                        name: "FK_company_details_AspNetUsers_EmployerId",
                        column: x => x.EmployerId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_company_details_EmployerId",
                table: "company_details",
                column: "EmployerId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "company_details");
        }
    }
}
