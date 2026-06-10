using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InstituteWebAPI.Migrations
{
    [DbContext(typeof(InstituteWebAPI.Data.RozhnInstituteDbContext))]
    [Migration("20260115120000_AddTerminalResultsTable")]
    public partial class AddTerminalResultsTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TerminalResults",
                columns: table => new
                {
                    TerminalResultID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TermID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CurrentClassID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudentID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Month3TestID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Month1TestID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IncludeMonth1 = table.Column<bool>(type: "bit", nullable: false),
                    Month2TestID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IncludeMonth2 = table.Column<bool>(type: "bit", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TerminalResults", x => x.TerminalResultID);
                    table.ForeignKey(
                        name: "FK_TerminalResults_CurrentClasses_CurrentClassID",
                        column: x => x.CurrentClassID,
                        principalTable: "CurrentClasses",
                        principalColumn: "CurrentClassID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TerminalResults_Students_StudentID",
                        column: x => x.StudentID,
                        principalTable: "Students",
                        principalColumn: "StudentID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TerminalResults_Term_TermID",
                        column: x => x.TermID,
                        principalTable: "Term",
                        principalColumn: "TermID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TerminalResults_CurrentClassID",
                table: "TerminalResults",
                column: "CurrentClassID");

            migrationBuilder.CreateIndex(
                name: "IX_TerminalResults_StudentID",
                table: "TerminalResults",
                column: "StudentID");

            migrationBuilder.CreateIndex(
                name: "IX_TerminalResults_TermID",
                table: "TerminalResults",
                column: "TermID");

            migrationBuilder.CreateIndex(
                name: "IX_TerminalResults_TermID_CurrentClassID_StudentID",
                table: "TerminalResults",
                columns: new[] { "TermID", "CurrentClassID", "StudentID" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TerminalResults");
        }
    }
}
