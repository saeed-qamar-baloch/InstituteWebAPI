using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InstituteWebAPI.Migrations
{
    /// <inheritdoc />
    public partial class SectionandSessionModifiedMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CurrentClasses_TimeSlots_TimeSlotID",
                table: "CurrentClasses");

            migrationBuilder.DropTable(
                name: "StudentCourses");

            migrationBuilder.DropTable(
                name: "TimeSlots");

            migrationBuilder.DropIndex(
                name: "IX_CurrentClasses_TimeSlotID",
                table: "CurrentClasses");

            migrationBuilder.DropColumn(
                name: "TimeSlotID",
                table: "CurrentClasses");

            migrationBuilder.AddColumn<string>(
                name: "Remarks",
                table: "Students",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "Admissions",
                columns: table => new
                {
                    AdmissionID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudentID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RegistrationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CourseID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LeavingDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Admissions", x => x.AdmissionID);
                    table.ForeignKey(
                        name: "FK_Admissions_Courses_CourseID",
                        column: x => x.CourseID,
                        principalTable: "Courses",
                        principalColumn: "CourseID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Admissions_Students_StudentID",
                        column: x => x.StudentID,
                        principalTable: "Students",
                        principalColumn: "StudentID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Sections",
                columns: table => new
                {
                    SectionID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SectionName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StartTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TermID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CourseID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SessionID = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sections", x => x.SectionID);
                    table.ForeignKey(
                        name: "FK_Sections_Courses_CourseID",
                        column: x => x.CourseID,
                        principalTable: "Courses",
                        principalColumn: "CourseID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Sections_Sessions_SessionID",
                        column: x => x.SessionID,
                        principalTable: "Sessions",
                        principalColumn: "SessionID");
                    table.ForeignKey(
                        name: "FK_Sections_Term_TermID",
                        column: x => x.TermID,
                        principalTable: "Term",
                        principalColumn: "TermID");
                });

            migrationBuilder.CreateIndex(
                name: "IX_CurrentClasses_SectionID",
                table: "CurrentClasses",
                column: "SectionID");

            migrationBuilder.CreateIndex(
                name: "IX_Admissions_CourseID",
                table: "Admissions",
                column: "CourseID");

            migrationBuilder.CreateIndex(
                name: "IX_Admissions_StudentID",
                table: "Admissions",
                column: "StudentID");

            migrationBuilder.CreateIndex(
                name: "IX_Sections_CourseID",
                table: "Sections",
                column: "CourseID");

            migrationBuilder.CreateIndex(
                name: "IX_Sections_SessionID",
                table: "Sections",
                column: "SessionID");

            migrationBuilder.CreateIndex(
                name: "IX_Sections_TermID",
                table: "Sections",
                column: "TermID");

            migrationBuilder.AddForeignKey(
                name: "FK_CurrentClasses_Sections_SectionID",
                table: "CurrentClasses",
                column: "SectionID",
                principalTable: "Sections",
                principalColumn: "SectionID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CurrentClasses_Sections_SectionID",
                table: "CurrentClasses");

            migrationBuilder.DropTable(
                name: "Admissions");

            migrationBuilder.DropTable(
                name: "Sections");

            migrationBuilder.DropIndex(
                name: "IX_CurrentClasses_SectionID",
                table: "CurrentClasses");

            migrationBuilder.DropColumn(
                name: "Remarks",
                table: "Students");

            migrationBuilder.AddColumn<Guid>(
                name: "TimeSlotID",
                table: "CurrentClasses",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "StudentCourses",
                columns: table => new
                {
                    StudentCourseID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CourseID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudentID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    LeavingDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RegistrationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentCourses", x => x.StudentCourseID);
                    table.ForeignKey(
                        name: "FK_StudentCourses_Courses_CourseID",
                        column: x => x.CourseID,
                        principalTable: "Courses",
                        principalColumn: "CourseID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StudentCourses_Students_StudentID",
                        column: x => x.StudentID,
                        principalTable: "Students",
                        principalColumn: "StudentID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TimeSlots",
                columns: table => new
                {
                    TimeSlotID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TermID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    EndTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StartTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TimeSlots", x => x.TimeSlotID);
                    table.ForeignKey(
                        name: "FK_TimeSlots_Term_TermID",
                        column: x => x.TermID,
                        principalTable: "Term",
                        principalColumn: "TermID");
                });

            migrationBuilder.CreateIndex(
                name: "IX_CurrentClasses_TimeSlotID",
                table: "CurrentClasses",
                column: "TimeSlotID");

            migrationBuilder.CreateIndex(
                name: "IX_StudentCourses_CourseID",
                table: "StudentCourses",
                column: "CourseID");

            migrationBuilder.CreateIndex(
                name: "IX_StudentCourses_StudentID",
                table: "StudentCourses",
                column: "StudentID");

            migrationBuilder.CreateIndex(
                name: "IX_TimeSlots_TermID",
                table: "TimeSlots",
                column: "TermID");

            migrationBuilder.AddForeignKey(
                name: "FK_CurrentClasses_TimeSlots_TimeSlotID",
                table: "CurrentClasses",
                column: "TimeSlotID",
                principalTable: "TimeSlots",
                principalColumn: "TimeSlotID");
        }
    }
}
