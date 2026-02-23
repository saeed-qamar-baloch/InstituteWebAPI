using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InstituteWebAPI.Migrations
{
    /// <inheritdoc />
    public partial class ChangeAdmissionDueDateToInt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Convert existing DueDate (datetime2) to an integer day-of-month safely.
            // Steps:
            // 1. Add a temporary int column.
            // 2. Populate it with DATEPART(day, DueDate) for existing rows.
            // 3. Drop the original DueDate column (datetime2).
            // 4. Add the new DueDate column as int and copy values from temp.
            // 5. Drop the temp column.

            migrationBuilder.AddColumn<int>(
                name: "DueDateTemp",
                table: "Admissions",
                type: "int",
                nullable: true);

            migrationBuilder.Sql("UPDATE Admissions SET DueDateTemp = DATEPART(day, DueDate) WHERE DueDate IS NOT NULL");

            migrationBuilder.DropColumn(
                name: "DueDate",
                table: "Admissions");

            migrationBuilder.AddColumn<int>(
                name: "DueDate",
                table: "Admissions",
                type: "int",
                nullable: true);

            migrationBuilder.Sql("UPDATE Admissions SET DueDate = DueDateTemp WHERE DueDateTemp IS NOT NULL");

            migrationBuilder.DropColumn(
                name: "DueDateTemp",
                table: "Admissions");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Reverse the change: convert integer day-of-month back to a datetime2 (approximate)
            // We'll create a temp datetime column and set a date using DATEFROMPARTS with current year/month
            // and the stored day-of-month. This is an approximation and may be adjusted as needed.
            migrationBuilder.AddColumn<DateTime>(
                name: "DueDateTemp",
                table: "Admissions",
                type: "datetime2",
                nullable: true);

            // Build a date using current year and month and the saved day. If the day is invalid for current month,
            // SQL will throw; we guard by using LEAST-like logic via CASE to clamp day to 28 when necessary.
            migrationBuilder.Sql(@"
UPDATE Admissions
SET DueDateTemp = CASE
    WHEN DueDate IS NULL THEN NULL
    ELSE DATEFROMPARTS(YEAR(GETDATE()), MONTH(GETDATE()), CASE WHEN DueDate > 28 THEN 28 ELSE DueDate END)
END
");

            migrationBuilder.DropColumn(
                name: "DueDate",
                table: "Admissions");

            migrationBuilder.AddColumn<DateTime>(
                name: "DueDate",
                table: "Admissions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.Sql("UPDATE Admissions SET DueDate = DueDateTemp WHERE DueDateTemp IS NOT NULL");

            migrationBuilder.DropColumn(
                name: "DueDateTemp",
                table: "Admissions");
        }
    }
}
