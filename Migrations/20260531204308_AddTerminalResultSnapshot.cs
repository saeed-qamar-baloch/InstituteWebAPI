using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InstituteWebAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddTerminalResultSnapshot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StudentLeaveRequests_Admissions_AdmissionID",
                table: "StudentLeaveRequests");

            migrationBuilder.AddColumn<string>(
                name: "Grade",
                table: "TerminalResults",
                type: "nvarchar(max)",
                nullable: true);

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

            migrationBuilder.AddForeignKey(
                name: "FK_StudentLeaveRequests_Admissions_AdmissionID",
                table: "StudentLeaveRequests",
                column: "AdmissionID",
                principalTable: "Admissions",
                principalColumn: "AdmissionID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StudentLeaveRequests_Admissions_AdmissionID",
                table: "StudentLeaveRequests");

            migrationBuilder.DropColumn(
                name: "Grade",
                table: "TerminalResults");

            migrationBuilder.DropColumn(
                name: "Month1ObtainedMarks",
                table: "TerminalResults");

            migrationBuilder.DropColumn(
                name: "Month1TotalMarks",
                table: "TerminalResults");

            migrationBuilder.DropColumn(
                name: "Month2ObtainedMarks",
                table: "TerminalResults");

            migrationBuilder.DropColumn(
                name: "Month2TotalMarks",
                table: "TerminalResults");

            migrationBuilder.DropColumn(
                name: "Month3ObtainedMarks",
                table: "TerminalResults");

            migrationBuilder.DropColumn(
                name: "Month3TotalMarks",
                table: "TerminalResults");

            migrationBuilder.AddForeignKey(
                name: "FK_StudentLeaveRequests_Admissions_AdmissionID",
                table: "StudentLeaveRequests",
                column: "AdmissionID",
                principalTable: "Admissions",
                principalColumn: "AdmissionID");
        }
    }
}
