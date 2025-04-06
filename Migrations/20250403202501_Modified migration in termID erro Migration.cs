using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InstituteWebAPI.Migrations
{
    /// <inheritdoc />
    public partial class ModifiedmigrationintermIDerroMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StudentMarks_Students_StudentID",
                table: "StudentMarks");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentMarks_Term_TermID",
                table: "StudentMarks");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentMarks_Tests_TestID",
                table: "StudentMarks");

            migrationBuilder.AlterColumn<Guid>(
                name: "TestID",
                table: "StudentMarks",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "TermID",
                table: "StudentMarks",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "StudentID",
                table: "StudentMarks",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_StudentMarks_Students_StudentID",
                table: "StudentMarks",
                column: "StudentID",
                principalTable: "Students",
                principalColumn: "StudentID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StudentMarks_Term_TermID",
                table: "StudentMarks",
                column: "TermID",
                principalTable: "Term",
                principalColumn: "TermID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StudentMarks_Tests_TestID",
                table: "StudentMarks",
                column: "TestID",
                principalTable: "Tests",
                principalColumn: "TestID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StudentMarks_Students_StudentID",
                table: "StudentMarks");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentMarks_Term_TermID",
                table: "StudentMarks");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentMarks_Tests_TestID",
                table: "StudentMarks");

            migrationBuilder.AlterColumn<Guid>(
                name: "TestID",
                table: "StudentMarks",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "TermID",
                table: "StudentMarks",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "StudentID",
                table: "StudentMarks",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddForeignKey(
                name: "FK_StudentMarks_Students_StudentID",
                table: "StudentMarks",
                column: "StudentID",
                principalTable: "Students",
                principalColumn: "StudentID");

            migrationBuilder.AddForeignKey(
                name: "FK_StudentMarks_Term_TermID",
                table: "StudentMarks",
                column: "TermID",
                principalTable: "Term",
                principalColumn: "TermID");

            migrationBuilder.AddForeignKey(
                name: "FK_StudentMarks_Tests_TestID",
                table: "StudentMarks",
                column: "TestID",
                principalTable: "Tests",
                principalColumn: "TestID");
        }
    }
}
