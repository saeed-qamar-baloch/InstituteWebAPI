using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InstituteWebAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddStudentLeaveRequests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StudentLeaveRequests",
                columns: table => new
                {
                    StudentLeaveRequestID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudentID             = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CurrentClassID        = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AdmissionID           = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RequestedByTeacherID  = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LeavingDate           = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Reason                = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status                = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    ReviewedByUserID      = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReviewedAt            = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReviewRemarks         = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt             = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt             = table.Column<DateTime>(type: "datetime2", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentLeaveRequests", x => x.StudentLeaveRequestID);

                    table.ForeignKey(
                        name: "FK_StudentLeaveRequests_Students_StudentID",
                        column: x => x.StudentID,
                        principalTable: "Students",
                        principalColumn: "StudentID",
                        onDelete: ReferentialAction.Cascade);

                    table.ForeignKey(
                        name: "FK_StudentLeaveRequests_CurrentClasses_CurrentClassID",
                        column: x => x.CurrentClassID,
                        principalTable: "CurrentClasses",
                        principalColumn: "CurrentClassID",
                        onDelete: ReferentialAction.Cascade);

                    table.ForeignKey(
                        name: "FK_StudentLeaveRequests_Admissions_AdmissionID",
                        column: x => x.AdmissionID,
                        principalTable: "Admissions",
                        principalColumn: "AdmissionID",
                        onDelete: ReferentialAction.NoAction);

                    table.ForeignKey(
                        name: "FK_StudentLeaveRequests_Teachers_RequestedByTeacherID",
                        column: x => x.RequestedByTeacherID,
                        principalTable: "Teachers",
                        principalColumn: "TeacherID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StudentLeaveRequests_StudentID",
                table: "StudentLeaveRequests",
                column: "StudentID");

            migrationBuilder.CreateIndex(
                name: "IX_StudentLeaveRequests_CurrentClassID",
                table: "StudentLeaveRequests",
                column: "CurrentClassID");

            migrationBuilder.CreateIndex(
                name: "IX_StudentLeaveRequests_AdmissionID",
                table: "StudentLeaveRequests",
                column: "AdmissionID");

            migrationBuilder.CreateIndex(
                name: "IX_StudentLeaveRequests_RequestedByTeacherID",
                table: "StudentLeaveRequests",
                column: "RequestedByTeacherID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "StudentLeaveRequests");
        }
    }
}
