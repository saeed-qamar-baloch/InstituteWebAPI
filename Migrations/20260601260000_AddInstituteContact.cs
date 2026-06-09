using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InstituteWebAPI.Migrations
{
    [DbContext(typeof(InstituteWebAPI.Data.RozhnInstituteDbContext))]
    [Migration("20260601260000_AddInstituteContact")]
    public partial class AddInstituteContact : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(name: "Tagline", table: "InstituteSettings", type: "nvarchar(max)", nullable: true);
            migrationBuilder.AddColumn<string>(name: "Address", table: "InstituteSettings", type: "nvarchar(max)", nullable: true);
            migrationBuilder.AddColumn<string>(name: "Phone", table: "InstituteSettings", type: "nvarchar(max)", nullable: true);
            migrationBuilder.AddColumn<string>(name: "Email", table: "InstituteSettings", type: "nvarchar(max)", nullable: true);
            migrationBuilder.AddColumn<string>(name: "Website", table: "InstituteSettings", type: "nvarchar(max)", nullable: true);
            migrationBuilder.AddColumn<string>(name: "SinceYear", table: "InstituteSettings", type: "nvarchar(max)", nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "Tagline", table: "InstituteSettings");
            migrationBuilder.DropColumn(name: "Address", table: "InstituteSettings");
            migrationBuilder.DropColumn(name: "Phone", table: "InstituteSettings");
            migrationBuilder.DropColumn(name: "Email", table: "InstituteSettings");
            migrationBuilder.DropColumn(name: "Website", table: "InstituteSettings");
            migrationBuilder.DropColumn(name: "SinceYear", table: "InstituteSettings");
        }
    }
}
