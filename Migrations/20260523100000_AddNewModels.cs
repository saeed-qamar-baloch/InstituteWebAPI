using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InstituteWebAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddNewModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ── 1. Add AdmittedClassID to Admissions ────────────────────────
            migrationBuilder.AddColumn<Guid>(
                name: "AdmittedClassID",
                table: "Admissions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Admissions_AdmittedClassID",
                table: "Admissions",
                column: "AdmittedClassID");

            migrationBuilder.AddForeignKey(
                name: "FK_Admissions_Classes_AdmittedClassID",
                table: "Admissions",
                column: "AdmittedClassID",
                principalTable: "Classes",
                principalColumn: "ClassID");

            // ── 2. Guardians ─────────────────────────────────────────────────
            migrationBuilder.CreateTable(
                name: "Guardians",
                columns: table => new
                {
                    GuardianID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudentID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GuardianName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Relation = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Contact = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Cnic = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Occupation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Guardians", x => x.GuardianID);
                    table.ForeignKey(
                        name: "FK_Guardians_Students_StudentID",
                        column: x => x.StudentID,
                        principalTable: "Students",
                        principalColumn: "StudentID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Guardians_StudentID",
                table: "Guardians",
                column: "StudentID");

            // ── 3. StudentFeeHistories ───────────────────────────────────────
            migrationBuilder.CreateTable(
                name: "StudentFeeHistories",
                columns: table => new
                {
                    FeeHistoryID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AdmissionID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CourseID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FeeAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    EffectiveFrom = table.Column<DateTime>(type: "date", nullable: false),
                    EffectiveTo = table.Column<DateTime>(type: "date", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentFeeHistories", x => x.FeeHistoryID);
                    table.ForeignKey(
                        name: "FK_StudentFeeHistories_Admissions_AdmissionID",
                        column: x => x.AdmissionID,
                        principalTable: "Admissions",
                        principalColumn: "AdmissionID");
                    table.ForeignKey(
                        name: "FK_StudentFeeHistories_Courses_CourseID",
                        column: x => x.CourseID,
                        principalTable: "Courses",
                        principalColumn: "CourseID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StudentFeeHistories_AdmissionID_EffectiveFrom",
                table: "StudentFeeHistories",
                columns: new[] { "AdmissionID", "EffectiveFrom" });

            // ── 4. Scholarships ──────────────────────────────────────────────
            migrationBuilder.CreateTable(
                name: "Scholarships",
                columns: table => new
                {
                    ScholarshipID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudentID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AdmissionID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DiscountPercent = table.Column<int>(type: "int", nullable: false),
                    FromMonth = table.Column<DateTime>(type: "date", nullable: false),
                    ToMonth = table.Column<DateTime>(type: "date", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedByUserID = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Scholarships", x => x.ScholarshipID);
                    table.ForeignKey(
                        name: "FK_Scholarships_Students_StudentID",
                        column: x => x.StudentID,
                        principalTable: "Students",
                        principalColumn: "StudentID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Scholarships_Admissions_AdmissionID",
                        column: x => x.AdmissionID,
                        principalTable: "Admissions",
                        principalColumn: "AdmissionID");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Scholarships_StudentID",
                table: "Scholarships",
                column: "StudentID");

            migrationBuilder.CreateIndex(
                name: "IX_Scholarships_AdmissionID",
                table: "Scholarships",
                column: "AdmissionID");

            // ── 5. ResultApprovals ───────────────────────────────────────────
            migrationBuilder.CreateTable(
                name: "ResultApprovals",
                columns: table => new
                {
                    ApprovalID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TermID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CurrentClassID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsApproved = table.Column<bool>(type: "bit", nullable: false),
                    ApprovedByUserID = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResultApprovals", x => x.ApprovalID);
                    table.ForeignKey(
                        name: "FK_ResultApprovals_Term_TermID",
                        column: x => x.TermID,
                        principalTable: "Term",
                        principalColumn: "TermID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ResultApprovals_CurrentClasses_CurrentClassID",
                        column: x => x.CurrentClassID,
                        principalTable: "CurrentClasses",
                        principalColumn: "CurrentClassID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ResultApprovals_TermID_CurrentClassID",
                table: "ResultApprovals",
                columns: new[] { "TermID", "CurrentClassID" },
                unique: true);

            // ── 6. MarkEditRequests ──────────────────────────────────────────
            migrationBuilder.CreateTable(
                name: "MarkEditRequests",
                columns: table => new
                {
                    RequestID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TeacherID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudentMarkID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CurrentMarks = table.Column<float>(type: "real", nullable: false),
                    RequestedMarks = table.Column<float>(type: "real", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ReviewedByUserID = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReviewedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReviewRemarks = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MarkEditRequests", x => x.RequestID);
                    table.ForeignKey(
                        name: "FK_MarkEditRequests_Teachers_TeacherID",
                        column: x => x.TeacherID,
                        principalTable: "Teachers",
                        principalColumn: "TeacherID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MarkEditRequests_StudentMarks_StudentMarkID",
                        column: x => x.StudentMarkID,
                        principalTable: "StudentMarks",
                        principalColumn: "StudentMarkID");
                });

            migrationBuilder.CreateIndex(
                name: "IX_MarkEditRequests_TeacherID",
                table: "MarkEditRequests",
                column: "TeacherID");

            migrationBuilder.CreateIndex(
                name: "IX_MarkEditRequests_StudentMarkID",
                table: "MarkEditRequests",
                column: "StudentMarkID");

            // ── 7. TeacherDailyAttendances ───────────────────────────────────
            migrationBuilder.CreateTable(
                name: "TeacherDailyAttendances",
                columns: table => new
                {
                    TeacherDailyAttendanceID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TeacherID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AttendanceDate = table.Column<DateTime>(type: "date", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ScannedBarcode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ScannedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MarkedByUserID = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeacherDailyAttendances", x => x.TeacherDailyAttendanceID);
                    table.ForeignKey(
                        name: "FK_TeacherDailyAttendances_Teachers_TeacherID",
                        column: x => x.TeacherID,
                        principalTable: "Teachers",
                        principalColumn: "TeacherID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TeacherDailyAttendances_TeacherID_AttendanceDate",
                table: "TeacherDailyAttendances",
                columns: new[] { "TeacherID", "AttendanceDate" },
                unique: true);

            // ── 8. TerminalPassingMarks ──────────────────────────────────────
            migrationBuilder.CreateTable(
                name: "TerminalPassingMarks",
                columns: table => new
                {
                    TerminalPassingMarkID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TermID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CurrentClassID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PassingMarks = table.Column<float>(type: "real", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TerminalPassingMarks", x => x.TerminalPassingMarkID);
                    table.ForeignKey(
                        name: "FK_TerminalPassingMarks_Term_TermID",
                        column: x => x.TermID,
                        principalTable: "Term",
                        principalColumn: "TermID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TerminalPassingMarks_CurrentClasses_CurrentClassID",
                        column: x => x.CurrentClassID,
                        principalTable: "CurrentClasses",
                        principalColumn: "CurrentClassID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TerminalPassingMarks_TermID_CurrentClassID",
                table: "TerminalPassingMarks",
                columns: new[] { "TermID", "CurrentClassID" },
                unique: true);

            // ── 9. Notifications ─────────────────────────────────────────────
            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    NotificationID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NotificationType = table.Column<int>(type: "int", nullable: false),
                    RecipientType = table.Column<int>(type: "int", nullable: false),
                    RecipientID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Channel = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    SentAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedByUserID = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.NotificationID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_RecipientID",
                table: "Notifications",
                column: "RecipientID");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_Status",
                table: "Notifications",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "Notifications");
            migrationBuilder.DropTable(name: "TerminalPassingMarks");
            migrationBuilder.DropTable(name: "TeacherDailyAttendances");
            migrationBuilder.DropTable(name: "MarkEditRequests");
            migrationBuilder.DropTable(name: "ResultApprovals");
            migrationBuilder.DropTable(name: "Scholarships");
            migrationBuilder.DropTable(name: "StudentFeeHistories");
            migrationBuilder.DropTable(name: "Guardians");

            migrationBuilder.DropForeignKey(
                name: "FK_Admissions_Classes_AdmittedClassID",
                table: "Admissions");

            migrationBuilder.DropIndex(
                name: "IX_Admissions_AdmittedClassID",
                table: "Admissions");

            migrationBuilder.DropColumn(
                name: "AdmittedClassID",
                table: "Admissions");
        }
    }
}
