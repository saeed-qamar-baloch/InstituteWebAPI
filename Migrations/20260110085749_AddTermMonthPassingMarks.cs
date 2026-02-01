using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InstituteWebAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddTermMonthPassingMarks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TermMonthPassingMarks",
                columns: table => new
                {
                    TermMonthPassingMarkID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TermMonthID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PassingMarks = table.Column<float>(type: "real", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TermMonthPassingMarks", x => x.TermMonthPassingMarkID);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TermMonthPassingMarks");
        }
    }
}
