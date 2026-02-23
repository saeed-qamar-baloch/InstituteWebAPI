using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InstituteWebAPI.Migrations
{
    [DbContext(typeof(InstituteWebAPI.Data.RozhnInstituteDbContext))]
    [Migration("20260303090000_AddFeeSettings")]
    public partial class AddFeeSettings : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FeeSettings",
                columns: table => new
                {
                    FeeSettingsId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LateFeeAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AdmissionFeeAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CardFeeAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeeSettings", x => x.FeeSettingsId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FeeSettings");
        }
    }
}
