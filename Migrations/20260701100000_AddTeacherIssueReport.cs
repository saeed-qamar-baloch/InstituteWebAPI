using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InstituteWebAPI.Migrations
{
    [DbContext(typeof(InstituteWebAPI.Data.RozhnInstituteDbContext))]
    [Migration("20260701100000_AddTeacherIssueReport")]
    public partial class AddTeacherIssueReport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "IssueReports",
                columns: table => new
                {
                    IssueId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TeacherId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CurrentClassId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IssueType = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    AdminNotes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IssueReports", x => x.IssueId);

                    table.ForeignKey(
                        name: "FK_IssueReports_Teachers_TeacherId",
                        column: x => x.TeacherId,
                        principalTable: "Teachers",
                        principalColumn: "TeacherID",
                        onDelete: ReferentialAction.NoAction);

                    table.ForeignKey(
                        name: "FK_IssueReports_CurrentClasses_CurrentClassId",
                        column: x => x.CurrentClassId,
                        principalTable: "CurrentClasses",
                        principalColumn: "CurrentClassID",
                        onDelete: ReferentialAction.NoAction);

                    table.ForeignKey(
                        name: "FK_IssueReports_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "StudentID",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateIndex(
                name: "IX_IssueReports_TeacherId",
                table: "IssueReports",
                column: "TeacherId");

            migrationBuilder.CreateIndex(
                name: "IX_IssueReports_CurrentClassId",
                table: "IssueReports",
                column: "CurrentClassId");

            migrationBuilder.CreateIndex(
                name: "IX_IssueReports_StudentId",
                table: "IssueReports",
                column: "StudentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "IssueReports");
        }
    }
}
