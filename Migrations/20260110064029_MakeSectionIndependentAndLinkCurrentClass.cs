using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InstituteWebAPI.Migrations
{
    /// <inheritdoc />
    public partial class MakeSectionIndependentAndLinkCurrentClass : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Sections_CurrentClasses_CurrentClassID",
                table: "Sections");

            migrationBuilder.DropIndex(
                name: "IX_Sections_CurrentClassID",
                table: "Sections");

            migrationBuilder.DropColumn(
                name: "CurrentClassID",
                table: "Sections");

            migrationBuilder.AddColumn<Guid>(
                name: "SectionID",
                table: "CurrentClasses",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CurrentClasses_SectionID",
                table: "CurrentClasses",
                column: "SectionID");

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

            migrationBuilder.DropIndex(
                name: "IX_CurrentClasses_SectionID",
                table: "CurrentClasses");

            migrationBuilder.DropColumn(
                name: "SectionID",
                table: "CurrentClasses");

            migrationBuilder.AddColumn<Guid>(
                name: "CurrentClassID",
                table: "Sections",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Sections_CurrentClassID",
                table: "Sections",
                column: "CurrentClassID");

            migrationBuilder.AddForeignKey(
                name: "FK_Sections_CurrentClasses_CurrentClassID",
                table: "Sections",
                column: "CurrentClassID",
                principalTable: "CurrentClasses",
                principalColumn: "CurrentClassID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
