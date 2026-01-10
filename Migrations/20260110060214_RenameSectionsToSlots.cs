using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InstituteWebAPI.Migrations
{
    /// <inheritdoc />
    public partial class RenameSectionsToSlots : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop old FK first
            migrationBuilder.DropForeignKey(
                name: "FK_CurrentClasses_Sections_SectionID",
                table: "CurrentClasses");

            // Rename dependent column in CurrentClasses
            migrationBuilder.RenameColumn(
                name: "SectionID",
                table: "CurrentClasses",
                newName: "SlotID");

            migrationBuilder.RenameIndex(
                name: "IX_CurrentClasses_SectionID",
                table: "CurrentClasses",
                newName: "IX_CurrentClasses_SlotID");

            // Rename Sections table to Slots (preserve data)
            migrationBuilder.RenameTable(
                name: "Sections",
                newName: "Slots");

            // Rename PK column + name column (preserve data)
            migrationBuilder.RenameColumn(
                name: "SectionID",
                table: "Slots",
                newName: "SlotID");

            migrationBuilder.RenameColumn(
                name: "SectionName",
                table: "Slots",
                newName: "SlotName");

            // Fix indexes on renamed table
            migrationBuilder.RenameIndex(
                name: "IX_Sections_CourseID",
                table: "Slots",
                newName: "IX_Slots_CourseID");

            migrationBuilder.RenameIndex(
                name: "IX_Sections_SessionID",
                table: "Slots",
                newName: "IX_Slots_SessionID");

            migrationBuilder.RenameIndex(
                name: "IX_Sections_TermID",
                table: "Slots",
                newName: "IX_Slots_TermID");

            // Re-add PK with new name
            migrationBuilder.DropPrimaryKey(
                name: "PK_Sections",
                table: "Slots");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Slots",
                table: "Slots",
                column: "SlotID");

            // Add new FK from CurrentClasses to Slots
            migrationBuilder.AddForeignKey(
                name: "FK_CurrentClasses_Slots_SlotID",
                table: "CurrentClasses",
                column: "SlotID",
                principalTable: "Slots",
                principalColumn: "SlotID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CurrentClasses_Slots_SlotID",
                table: "CurrentClasses");

            // Rename dependent column back
            migrationBuilder.RenameColumn(
                name: "SlotID",
                table: "CurrentClasses",
                newName: "SectionID");

            migrationBuilder.RenameIndex(
                name: "IX_CurrentClasses_SlotID",
                table: "CurrentClasses",
                newName: "IX_CurrentClasses_SectionID");

            // Revert PK name
            migrationBuilder.DropPrimaryKey(
                name: "PK_Slots",
                table: "Slots");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Sections",
                table: "Slots",
                column: "SlotID");

            // Revert indexes names
            migrationBuilder.RenameIndex(
                name: "IX_Slots_CourseID",
                table: "Slots",
                newName: "IX_Sections_CourseID");

            migrationBuilder.RenameIndex(
                name: "IX_Slots_SessionID",
                table: "Slots",
                newName: "IX_Sections_SessionID");

            migrationBuilder.RenameIndex(
                name: "IX_Slots_TermID",
                table: "Slots",
                newName: "IX_Sections_TermID");

            // Rename columns back
            migrationBuilder.RenameColumn(
                name: "SlotID",
                table: "Slots",
                newName: "SectionID");

            migrationBuilder.RenameColumn(
                name: "SlotName",
                table: "Slots",
                newName: "SectionName");

            // Rename table back
            migrationBuilder.RenameTable(
                name: "Slots",
                newName: "Sections");

            // Re-add FK
            migrationBuilder.AddForeignKey(
                name: "FK_CurrentClasses_Sections_SectionID",
                table: "CurrentClasses",
                column: "SectionID",
                principalTable: "Sections",
                principalColumn: "SectionID");
        }
    }
}
