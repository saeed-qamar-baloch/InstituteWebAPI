using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InstituteWebAPI.Migrations
{
    [DbContext(typeof(InstituteWebAPI.Data.RozhnInstituteDbContext))]
    [Migration("20260601130000_MakeAttendanceMarkedByTeacherNullable")]
    public partial class MakeAttendanceMarkedByTeacherNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop the existing (cascade) FK so we can change the column to nullable.
            migrationBuilder.DropForeignKey(
                name: "FK_StudentAttendances_Teachers_MarkedByTeacherID",
                table: "StudentAttendances");

            migrationBuilder.AlterColumn<System.Guid>(
                name: "MarkedByTeacherID",
                table: "StudentAttendances",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(System.Guid),
                oldType: "uniqueidentifier");

            // Re-add the FK; allow null (Admin-marked) and avoid cascade delete on teachers.
            migrationBuilder.AddForeignKey(
                name: "FK_StudentAttendances_Teachers_MarkedByTeacherID",
                table: "StudentAttendances",
                column: "MarkedByTeacherID",
                principalTable: "Teachers",
                principalColumn: "TeacherID",
                onDelete: ReferentialAction.NoAction);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StudentAttendances_Teachers_MarkedByTeacherID",
                table: "StudentAttendances");

            migrationBuilder.AlterColumn<System.Guid>(
                name: "MarkedByTeacherID",
                table: "StudentAttendances",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: System.Guid.Empty,
                oldClrType: typeof(System.Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_StudentAttendances_Teachers_MarkedByTeacherID",
                table: "StudentAttendances",
                column: "MarkedByTeacherID",
                principalTable: "Teachers",
                principalColumn: "TeacherID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
