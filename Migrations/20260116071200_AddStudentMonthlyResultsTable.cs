using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InstituteWebAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddStudentMonthlyResultsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StudentMonthlyResults",
                columns: table => new
                {
                    StudentMonthlyResultID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TermID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CurrentClassID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TermMonthID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudentID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TotalMarks = table.Column<float>(type: "real", nullable: false),
                    ObtainedMarks = table.Column<float>(type: "real", nullable: false),
                    Percentage = table.Column<float>(type: "real", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentMonthlyResults", x => x.StudentMonthlyResultID);
                    table.ForeignKey(
                        name: "FK_StudentMonthlyResults_CurrentClasses_CurrentClassID",
                        column: x => x.CurrentClassID,
                        principalTable: "CurrentClasses",
                        principalColumn: "CurrentClassID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StudentMonthlyResults_Students_StudentID",
                        column: x => x.StudentID,
                        principalTable: "Students",
                        principalColumn: "StudentID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StudentMonthlyResults_TermMonths_TermMonthID",
                        column: x => x.TermMonthID,
                        principalTable: "TermMonths",
                        principalColumn: "TermMonthID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StudentMonthlyResults_Term_TermID",
                        column: x => x.TermID,
                        principalTable: "Term",
                        principalColumn: "TermID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StudentMonthlyResults_CurrentClassID",
                table: "StudentMonthlyResults",
                column: "CurrentClassID");

            migrationBuilder.CreateIndex(
                name: "IX_StudentMonthlyResults_StudentID",
                table: "StudentMonthlyResults",
                column: "StudentID");

            migrationBuilder.CreateIndex(
                name: "IX_StudentMonthlyResults_TermID_CurrentClassID_TermMonthID_StudentID",
                table: "StudentMonthlyResults",
                columns: new[] { "TermID", "CurrentClassID", "TermMonthID", "StudentID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StudentMonthlyResults_TermMonthID",
                table: "StudentMonthlyResults",
                column: "TermMonthID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StudentMonthlyResults");
        }
    }
}
