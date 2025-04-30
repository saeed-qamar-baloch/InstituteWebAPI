using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InstituteWebAPI.Migrations.RozhnInstituteAuthDb
{
    /// <inheritdoc />
    public partial class RolesChangesMigration2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "5d0033a8-20e7-4997-8f3b-5769ec3b040b",
                column: "NormalizedName",
                value: "PARENT");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "5d0033a8-20e7-4997-8f3b-5769ec3b040b",
                column: "NormalizedName",
                value: "PAREN");
        }
    }
}
