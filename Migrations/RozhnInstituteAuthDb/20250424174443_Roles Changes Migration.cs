using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace InstituteWebAPI.Migrations.RozhnInstituteAuthDb
{
    /// <inheritdoc />
    public partial class RolesChangesMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "5d0033a8-20e7-4997-8f3b-5769ec3b040b",
                columns: new[] { "Name", "NormalizedName" },
                values: new object[] { "Parent", "PAREN" });

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "f4969356-5492-442e-bbc2-d7128a5206ab",
                columns: new[] { "Name", "NormalizedName" },
                values: new object[] { "Admin", "ADMIN" });

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "18ff3f63-158f-4a29-9963-e5a3db80cb51", "18ff3f63-158f-4a29-9963-e5a3db80cb51", "Student", "STUDENT" },
                    { "a551991f-c51c-4149-a7cd-3787b6d727e2", "a551991f-c51c-4149-a7cd-3787b6d727e2", "Teacher", "TEACHER" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "18ff3f63-158f-4a29-9963-e5a3db80cb51");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "a551991f-c51c-4149-a7cd-3787b6d727e2");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "5d0033a8-20e7-4997-8f3b-5769ec3b040b",
                columns: new[] { "Name", "NormalizedName" },
                values: new object[] { "Reader", "READER" });

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "f4969356-5492-442e-bbc2-d7128a5206ab",
                columns: new[] { "Name", "NormalizedName" },
                values: new object[] { "Writer", "WRITER" });
        }
    }
}
