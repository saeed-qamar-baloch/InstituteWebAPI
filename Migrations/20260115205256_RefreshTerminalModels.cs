using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InstituteWebAPI.Migrations
{
    /// <inheritdoc />
    public partial class RefreshTerminalModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TerminalIncludeSettings");

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TerminalResults");

            migrationBuilder.CreateTable(
                name: "TerminalIncludeSettings",
                columns: table => new
                {
                    TerminalIncludeSettingID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CurrentClassID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudentID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TermID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IncludeMonth1 = table.Column<bool>(type: "bit", nullable: false),
                    IncludeMonth1TestID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IncludeMonth2 = table.Column<bool>(type: "bit", nullable: false),
                    IncludeMonth2TestID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Month3TestID = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TerminalIncludeSettings", x => x.TerminalIncludeSettingID);
                    table.ForeignKey(
                        name: "FK_TerminalIncludeSettings_CurrentClasses_CurrentClassID",
                        column: x => x.CurrentClassID,
                        principalTable: "CurrentClasses",
                        principalColumn: "CurrentClassID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TerminalIncludeSettings_Students_StudentID",
                        column: x => x.StudentID,
                        principalTable: "Students",
                        principalColumn: "StudentID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TerminalIncludeSettings_Term_TermID",
                        column: x => x.TermID,
                        principalTable: "Term",
                        principalColumn: "TermID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TerminalIncludeSettings_CurrentClassID",
                table: "TerminalIncludeSettings",
                column: "CurrentClassID");

            migrationBuilder.CreateIndex(
                name: "IX_TerminalIncludeSettings_StudentID",
                table: "TerminalIncludeSettings",
                column: "StudentID");

            migrationBuilder.CreateIndex(
                name: "IX_TerminalIncludeSettings_TermID",
                table: "TerminalIncludeSettings",
                column: "TermID");
        }
    }
}
