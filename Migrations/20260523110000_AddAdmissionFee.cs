using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InstituteWebAPI.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(InstituteWebAPI.Data.RozhnInstituteDbContext))]
    [Migration("20260523110000_AddAdmissionFee")]
    public partial class AddAdmissionFee : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "AdmissionFee",
                table: "Admissions",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AdmissionFee",
                table: "Admissions");
        }
    }
}
