using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InstituteWebAPI.Migrations
{
    [DbContext(typeof(InstituteWebAPI.Data.RozhnInstituteDbContext))]
    [Migration("20260601240000_AddInstituteInfo")]
    public partial class AddInstituteInfo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(name: "InstituteName", table: "InstituteSettings", type: "nvarchar(max)", nullable: true);
            migrationBuilder.AddColumn<string>(name: "LogoUrl", table: "InstituteSettings", type: "nvarchar(max)", nullable: true);
            migrationBuilder.Sql("UPDATE InstituteSettings SET InstituteName = N'Rozhn Institute' WHERE InstituteName IS NULL;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "InstituteName", table: "InstituteSettings");
            migrationBuilder.DropColumn(name: "LogoUrl", table: "InstituteSettings");
        }
    }
}
