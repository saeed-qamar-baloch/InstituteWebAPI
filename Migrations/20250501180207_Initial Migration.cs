using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InstituteWebAPI.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Courses",
                columns: table => new
                {
                    CourseID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CourseName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CourseDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CourseStatus = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Courses", x => x.CourseID);
                });

            migrationBuilder.CreateTable(
                name: "Sessions",
                columns: table => new
                {
                    SessionID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SessionName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SessionStartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SessionEndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sessions", x => x.SessionID);
                });

            migrationBuilder.CreateTable(
                name: "Teachers",
                columns: table => new
                {
                    TeacherID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Serial = table.Column<int>(type: "int", nullable: false),
                    RegistrationNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TeacherName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FatherName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Gender = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateOfBirth = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    City = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Region = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EmergencyContact = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Contact = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FatherOccupation = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Qualification = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Institute = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Cnic = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Picture = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Experience = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RegistrationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsTeaching = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Teachers", x => x.TeacherID);
                });

            migrationBuilder.CreateTable(
                name: "Term",
                columns: table => new
                {
                    TermID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TermName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TermStart = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TermEnd = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TermDuration = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Term", x => x.TermID);
                });

            migrationBuilder.CreateTable(
                name: "TermMonths",
                columns: table => new
                {
                    TermMonthID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TermMonth = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TermMonths", x => x.TermMonthID);
                });

            migrationBuilder.CreateTable(
                name: "Village",
                columns: table => new
                {
                    VillageID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VillageName = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Village", x => x.VillageID);
                });

            migrationBuilder.CreateTable(
                name: "Classes",
                columns: table => new
                {
                    ClassID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClassName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CourseID = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Classes", x => x.ClassID);
                    table.ForeignKey(
                        name: "FK_Classes_Courses_CourseID",
                        column: x => x.CourseID,
                        principalTable: "Courses",
                        principalColumn: "CourseID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TeacherCourses",
                columns: table => new
                {
                    TeacherCourseID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TeacherID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CourseID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CourseIsTaken = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeacherCourses", x => x.TeacherCourseID);
                    table.ForeignKey(
                        name: "FK_TeacherCourses_Courses_CourseID",
                        column: x => x.CourseID,
                        principalTable: "Courses",
                        principalColumn: "CourseID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TeacherCourses_Teachers_TeacherID",
                        column: x => x.TeacherID,
                        principalTable: "Teachers",
                        principalColumn: "TeacherID",
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

            migrationBuilder.CreateTable(
                name: "Students",
                columns: table => new
                {
                    StudentID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Serial = table.Column<int>(type: "int", nullable: false),
                    RegDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RegistrationNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StudentName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FatherName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Gender = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateOfBirth = table.Column<DateTime>(type: "datetime2", nullable: false),
                    VillageID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    City = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FatherContact = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StudentContact = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FatherOccupation = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Qualification = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Institute = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FatherCnic = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Picture = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsEnrolled = table.Column<bool>(type: "bit", nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Students", x => x.StudentID);
                    table.ForeignKey(
                        name: "FK_Students_Village_VillageID",
                        column: x => x.VillageID,
                        principalTable: "Village",
                        principalColumn: "VillageID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CurrentClasses",
                columns: table => new
                {
                    CurrentClassID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClassID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SectionID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TeacherID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SessionID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TermID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CurrentClasses", x => x.CurrentClassID);
                    table.ForeignKey(
                        name: "FK_CurrentClasses_Classes_ClassID",
                        column: x => x.ClassID,
                        principalTable: "Classes",
                        principalColumn: "ClassID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CurrentClasses_Sections_SectionID",
                        column: x => x.SectionID,
                        principalTable: "Sections",
                        principalColumn: "SectionID");
                    table.ForeignKey(
                        name: "FK_CurrentClasses_Sessions_SessionID",
                        column: x => x.SessionID,
                        principalTable: "Sessions",
                        principalColumn: "SessionID");
                    table.ForeignKey(
                        name: "FK_CurrentClasses_Teachers_TeacherID",
                        column: x => x.TeacherID,
                        principalTable: "Teachers",
                        principalColumn: "TeacherID");
                    table.ForeignKey(
                        name: "FK_CurrentClasses_Term_TermID",
                        column: x => x.TermID,
                        principalTable: "Term",
                        principalColumn: "TermID");
                });

            migrationBuilder.CreateTable(
                name: "Admissions",
                columns: table => new
                {
                    AdmissionID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudentID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RegistrationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CourseID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LeavingDate = table.Column<DateTime>(type: "datetime2", nullable: true),
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
                name: "ClassStudents",
                columns: table => new
                {
                    ClassStudentID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CurrentClassID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudentID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClassStudents", x => x.ClassStudentID);
                    table.ForeignKey(
                        name: "FK_ClassStudents_CurrentClasses_CurrentClassID",
                        column: x => x.CurrentClassID,
                        principalTable: "CurrentClasses",
                        principalColumn: "CurrentClassID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClassStudents_Students_StudentID",
                        column: x => x.StudentID,
                        principalTable: "Students",
                        principalColumn: "StudentID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tests",
                columns: table => new
                {
                    TestID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TermMonthID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TestType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TotalMarks = table.Column<float>(type: "real", nullable: false),
                    CurrentClassID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TermID = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tests", x => x.TestID);
                    table.ForeignKey(
                        name: "FK_Tests_CurrentClasses_CurrentClassID",
                        column: x => x.CurrentClassID,
                        principalTable: "CurrentClasses",
                        principalColumn: "CurrentClassID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Tests_TermMonths_TermMonthID",
                        column: x => x.TermMonthID,
                        principalTable: "TermMonths",
                        principalColumn: "TermMonthID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Tests_Term_TermID",
                        column: x => x.TermID,
                        principalTable: "Term",
                        principalColumn: "TermID");
                });

            migrationBuilder.CreateTable(
                name: "StudentMarks",
                columns: table => new
                {
                    StudentMarkID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ObtainedMarks = table.Column<float>(type: "real", nullable: false),
                    TestID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudentID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TermID = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentMarks", x => x.StudentMarkID);
                    table.ForeignKey(
                        name: "FK_StudentMarks_Students_StudentID",
                        column: x => x.StudentID,
                        principalTable: "Students",
                        principalColumn: "StudentID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StudentMarks_Term_TermID",
                        column: x => x.TermID,
                        principalTable: "Term",
                        principalColumn: "TermID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StudentMarks_Tests_TestID",
                        column: x => x.TestID,
                        principalTable: "Tests",
                        principalColumn: "TestID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Admissions_CourseID",
                table: "Admissions",
                column: "CourseID");

            migrationBuilder.CreateIndex(
                name: "IX_Admissions_StudentID",
                table: "Admissions",
                column: "StudentID");

            migrationBuilder.CreateIndex(
                name: "IX_Classes_CourseID",
                table: "Classes",
                column: "CourseID");

            migrationBuilder.CreateIndex(
                name: "IX_ClassStudents_CurrentClassID",
                table: "ClassStudents",
                column: "CurrentClassID");

            migrationBuilder.CreateIndex(
                name: "IX_ClassStudents_StudentID",
                table: "ClassStudents",
                column: "StudentID");

            migrationBuilder.CreateIndex(
                name: "IX_CurrentClasses_ClassID",
                table: "CurrentClasses",
                column: "ClassID");

            migrationBuilder.CreateIndex(
                name: "IX_CurrentClasses_SectionID",
                table: "CurrentClasses",
                column: "SectionID");

            migrationBuilder.CreateIndex(
                name: "IX_CurrentClasses_SessionID",
                table: "CurrentClasses",
                column: "SessionID");

            migrationBuilder.CreateIndex(
                name: "IX_CurrentClasses_TeacherID",
                table: "CurrentClasses",
                column: "TeacherID");

            migrationBuilder.CreateIndex(
                name: "IX_CurrentClasses_TermID",
                table: "CurrentClasses",
                column: "TermID");

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

            migrationBuilder.CreateIndex(
                name: "IX_StudentMarks_StudentID",
                table: "StudentMarks",
                column: "StudentID");

            migrationBuilder.CreateIndex(
                name: "IX_StudentMarks_TermID",
                table: "StudentMarks",
                column: "TermID");

            migrationBuilder.CreateIndex(
                name: "IX_StudentMarks_TestID",
                table: "StudentMarks",
                column: "TestID");

            migrationBuilder.CreateIndex(
                name: "IX_Students_VillageID",
                table: "Students",
                column: "VillageID");

            migrationBuilder.CreateIndex(
                name: "IX_TeacherCourses_CourseID",
                table: "TeacherCourses",
                column: "CourseID");

            migrationBuilder.CreateIndex(
                name: "IX_TeacherCourses_TeacherID",
                table: "TeacherCourses",
                column: "TeacherID");

            migrationBuilder.CreateIndex(
                name: "IX_Tests_CurrentClassID",
                table: "Tests",
                column: "CurrentClassID");

            migrationBuilder.CreateIndex(
                name: "IX_Tests_TermID",
                table: "Tests",
                column: "TermID");

            migrationBuilder.CreateIndex(
                name: "IX_Tests_TermMonthID",
                table: "Tests",
                column: "TermMonthID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Admissions");

            migrationBuilder.DropTable(
                name: "ClassStudents");

            migrationBuilder.DropTable(
                name: "StudentMarks");

            migrationBuilder.DropTable(
                name: "TeacherCourses");

            migrationBuilder.DropTable(
                name: "Students");

            migrationBuilder.DropTable(
                name: "Tests");

            migrationBuilder.DropTable(
                name: "Village");

            migrationBuilder.DropTable(
                name: "CurrentClasses");

            migrationBuilder.DropTable(
                name: "TermMonths");

            migrationBuilder.DropTable(
                name: "Classes");

            migrationBuilder.DropTable(
                name: "Sections");

            migrationBuilder.DropTable(
                name: "Teachers");

            migrationBuilder.DropTable(
                name: "Courses");

            migrationBuilder.DropTable(
                name: "Sessions");

            migrationBuilder.DropTable(
                name: "Term");
        }
    }
}
