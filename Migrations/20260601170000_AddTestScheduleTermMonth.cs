using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InstituteWebAPI.Migrations
{
    [DbContext(typeof(InstituteWebAPI.Data.RozhnInstituteDbContext))]
    [Migration("20260601170000_AddTestScheduleTermMonth")]
    public partial class AddTestScheduleTermMonth : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "TermMonthID",
                table: "TestSchedules",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TestSchedules_TermMonthID",
                table: "TestSchedules",
                column: "TermMonthID");

            migrationBuilder.AddForeignKey(
                name: "FK_TestSchedules_TermMonths_TermMonthID",
                table: "TestSchedules",
                column: "TermMonthID",
                principalTable: "TermMonths",
                principalColumn: "TermMonthID",
                onDelete: ReferentialAction.NoAction);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TestSchedules_TermMonths_TermMonthID",
                table: "TestSchedules");

            migrationBuilder.DropIndex(
                name: "IX_TestSchedules_TermMonthID",
                table: "TestSchedules");

            migrationBuilder.DropColumn(
                name: "TermMonthID",
                table: "TestSchedules");
        }
    }
}
