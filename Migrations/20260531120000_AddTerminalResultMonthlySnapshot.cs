using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InstituteWebAPI.Migrations
{
    // NOTE: No [Migration] attribute — superseded by AddTerminalResultSnapshot (20260531204308)
    // which adds the same columns. Keeping this visible causes duplicate-column error 2705.
    public partial class AddTerminalResultMonthlySnapshot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<float>(
                name: "Month1ObtainedMarks",
                table: "TerminalResults",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<float>(
                name: "Month1TotalMarks",
                table: "TerminalResults",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<float>(
                name: "Month2ObtainedMarks",
                table: "TerminalResults",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<float>(
                name: "Month2TotalMarks",
                table: "TerminalResults",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<float>(
                name: "Month3ObtainedMarks",
                table: "TerminalResults",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<float>(
                name: "Month3TotalMarks",
                table: "TerminalResults",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<string>(
                name: "Grade",
                table: "TerminalResults",
                type: "nvarchar(5)",
                maxLength: 5,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "Month1ObtainedMarks", table: "TerminalResults");
            migrationBuilder.DropColumn(name: "Month1TotalMarks",    table: "TerminalResults");
            migrationBuilder.DropColumn(name: "Month2ObtainedMarks", table: "TerminalResults");
            migrationBuilder.DropColumn(name: "Month2TotalMarks",    table: "TerminalResults");
            migrationBuilder.DropColumn(name: "Month3ObtainedMarks", table: "TerminalResults");
            migrationBuilder.DropColumn(name: "Month3TotalMarks",    table: "TerminalResults");
            migrationBuilder.DropColumn(name: "Grade",               table: "TerminalResults");
        }
    }
}
