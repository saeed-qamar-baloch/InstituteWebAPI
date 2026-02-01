using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InstituteWebAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddStudentAttendanceTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StudentAttendances",
                columns: table => new
                {
                    StudentAttendanceID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AttendanceDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CurrentClassID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudentID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    MarkedByTeacherID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentAttendances", x => x.StudentAttendanceID);
                    table.ForeignKey(
                        name: "FK_StudentAttendances_CurrentClasses_CurrentClassID",
                        column: x => x.CurrentClassID,
                        principalTable: "CurrentClasses",
                        principalColumn: "CurrentClassID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StudentAttendances_Students_StudentID",
                        column: x => x.StudentID,
                        principalTable: "Students",
                        principalColumn: "StudentID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StudentAttendances_Teachers_MarkedByTeacherID",
                        column: x => x.MarkedByTeacherID,
                        principalTable: "Teachers",
                        principalColumn: "TeacherID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StudentAttendances_AttendanceDate_CurrentClassID_StudentID",
                table: "StudentAttendances",
                columns: new[] { "AttendanceDate", "CurrentClassID", "StudentID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StudentAttendances_CurrentClassID",
                table: "StudentAttendances",
                column: "CurrentClassID");

            migrationBuilder.CreateIndex(
                name: "IX_StudentAttendances_MarkedByTeacherID",
                table: "StudentAttendances",
                column: "MarkedByTeacherID");

            migrationBuilder.CreateIndex(
                name: "IX_StudentAttendances_StudentID",
                table: "StudentAttendances",
                column: "StudentID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StudentAttendances");
        }
    }
}
