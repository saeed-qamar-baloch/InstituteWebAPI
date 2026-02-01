using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InstituteWebAPI.Migrations
{
    /// <inheritdoc />
    public partial class SyncTerminalResultModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TerminalResults_TermID",
                table: "TerminalResults");

            migrationBuilder.AddColumn<float>(
                name: "Percentage",
                table: "TerminalResults",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<string>(
                name: "Result",
                table: "TerminalResults",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "TotalMarksConsidered",
                table: "TerminalResults",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<float>(
                name: "TotalObtained",
                table: "TerminalResults",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.CreateIndex(
                name: "IX_TerminalResults_TermID_CurrentClassID_StudentID",
                table: "TerminalResults",
                columns: new[] { "TermID", "CurrentClassID", "StudentID" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TerminalResults_TermID_CurrentClassID_StudentID",
                table: "TerminalResults");

            migrationBuilder.DropColumn(
                name: "Percentage",
                table: "TerminalResults");

            migrationBuilder.DropColumn(
                name: "Result",
                table: "TerminalResults");

            migrationBuilder.DropColumn(
                name: "TotalMarksConsidered",
                table: "TerminalResults");

            migrationBuilder.DropColumn(
                name: "TotalObtained",
                table: "TerminalResults");

            migrationBuilder.CreateIndex(
                name: "IX_TerminalResults_TermID",
                table: "TerminalResults",
                column: "TermID");
        }
    }
}
