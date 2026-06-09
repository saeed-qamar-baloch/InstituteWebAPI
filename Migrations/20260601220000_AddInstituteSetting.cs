using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InstituteWebAPI.Migrations
{
    [DbContext(typeof(InstituteWebAPI.Data.RozhnInstituteDbContext))]
    [Migration("20260601220000_AddInstituteSetting")]
    public partial class AddInstituteSetting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InstituteSettings",
                columns: table => new
                {
                    InstituteSettingID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OffDays = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InstituteSettings", x => x.InstituteSettingID);
                });

            // Seed single row, default Sunday off (7)
            migrationBuilder.Sql(
                "INSERT INTO InstituteSettings (InstituteSettingID, OffDays) VALUES ('d1000000-0000-0000-0000-000000000001', N'7');");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "InstituteSettings");
        }
    }
}
