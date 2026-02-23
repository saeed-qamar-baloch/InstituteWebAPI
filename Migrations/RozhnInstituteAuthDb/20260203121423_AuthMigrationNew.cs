using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace InstituteWebAPI.Migrations.RozhnInstituteAuthDb
{
    /// <inheritdoc />
    public partial class AuthMigrationNew : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "18ff3f63-158f-4a29-9963-e5a3db80cb51");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "5d0033a8-20e7-4997-8f3b-5769ec3b040b");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "18ff3f63-158f-4a29-9963-e5a3db80cb51", "18ff3f63-158f-4a29-9963-e5a3db80cb51", "Student", "STUDENT" },
                    { "5d0033a8-20e7-4997-8f3b-5769ec3b040b", "5d0033a8-20e7-4997-8f3b-5769ec3b040b", "Parent", "PARENT" }
                });
        }
    }
}
