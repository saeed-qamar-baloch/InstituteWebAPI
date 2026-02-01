using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InstituteWebAPI.Migrations
{
    /// <inheritdoc />
    public partial class ScopeTermMonthPassingMarksByTermAndClass : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1) Add as nullable to avoid breaking existing rows
            migrationBuilder.AddColumn<Guid>(
                name: "CurrentClassID",
                table: "TermMonthPassingMarks",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TermID",
                table: "TermMonthPassingMarks",
                type: "uniqueidentifier",
                nullable: true);

            // 2) Best-effort backfill for existing data
            // If there is an active term, use it; otherwise use any term.
            // For class, use any CurrentClass (preferably in active term if possible).
            migrationBuilder.Sql(@"
DECLARE @ActiveTerm uniqueidentifier;
SELECT TOP(1) @ActiveTerm = TermID FROM Term WHERE IsActive = 1;
IF (@ActiveTerm IS NULL)
    SELECT TOP(1) @ActiveTerm = TermID FROM Term ORDER BY TermStart DESC;

DECLARE @AnyClass uniqueidentifier;
SELECT TOP(1) @AnyClass = CurrentClassID FROM CurrentClasses WHERE TermID = @ActiveTerm;
IF (@AnyClass IS NULL)
    SELECT TOP(1) @AnyClass = CurrentClassID FROM CurrentClasses;

UPDATE TermMonthPassingMarks
SET TermID = ISNULL(TermID, @ActiveTerm),
    CurrentClassID = ISNULL(CurrentClassID, @AnyClass)
WHERE TermID IS NULL OR CurrentClassID IS NULL;
");

            // 3) Now make non-nullable
            migrationBuilder.AlterColumn<Guid>(
                name: "CurrentClassID",
                table: "TermMonthPassingMarks",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true,
                defaultValue: Guid.Empty);

            migrationBuilder.AlterColumn<Guid>(
                name: "TermID",
                table: "TermMonthPassingMarks",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true,
                defaultValue: Guid.Empty);

            // 4) Indexes + uniqueness
            migrationBuilder.CreateIndex(
                name: "IX_TermMonthPassingMarks_CurrentClassID",
                table: "TermMonthPassingMarks",
                column: "CurrentClassID");

            migrationBuilder.CreateIndex(
                name: "IX_TermMonthPassingMarks_TermID",
                table: "TermMonthPassingMarks",
                column: "TermID");

            migrationBuilder.CreateIndex(
                name: "IX_TermMonthPassingMarks_TermID_CurrentClassID_TermMonthID",
                table: "TermMonthPassingMarks",
                columns: new[] { "TermID", "CurrentClassID", "TermMonthID" },
                unique: true);

            // 5) Foreign keys
            migrationBuilder.AddForeignKey(
                name: "FK_TermMonthPassingMarks_CurrentClasses_CurrentClassID",
                table: "TermMonthPassingMarks",
                column: "CurrentClassID",
                principalTable: "CurrentClasses",
                principalColumn: "CurrentClassID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TermMonthPassingMarks_Term_TermID",
                table: "TermMonthPassingMarks",
                column: "TermID",
                principalTable: "Term",
                principalColumn: "TermID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TermMonthPassingMarks_CurrentClasses_CurrentClassID",
                table: "TermMonthPassingMarks");

            migrationBuilder.DropForeignKey(
                name: "FK_TermMonthPassingMarks_Term_TermID",
                table: "TermMonthPassingMarks");

            migrationBuilder.DropIndex(
                name: "IX_TermMonthPassingMarks_TermID_CurrentClassID_TermMonthID",
                table: "TermMonthPassingMarks");

            migrationBuilder.DropIndex(
                name: "IX_TermMonthPassingMarks_CurrentClassID",
                table: "TermMonthPassingMarks");

            migrationBuilder.DropIndex(
                name: "IX_TermMonthPassingMarks_TermID",
                table: "TermMonthPassingMarks");

            migrationBuilder.DropColumn(
                name: "CurrentClassID",
                table: "TermMonthPassingMarks");

            migrationBuilder.DropColumn(
                name: "TermID",
                table: "TermMonthPassingMarks");
        }
    }
}
