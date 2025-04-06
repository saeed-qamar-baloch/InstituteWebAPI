using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InstituteWebAPI.Migrations
{
    /// <inheritdoc />
    public partial class SessionandSectionremovedMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CurrentClasses_Sections_SectionID",
                table: "CurrentClasses");

            migrationBuilder.DropTable(
                name: "Sections");

            migrationBuilder.DropIndex(
                name: "IX_CurrentClasses_SectionID",
                table: "CurrentClasses");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Sections",
                columns: table => new
                {
                    SectionID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CourseID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    SectionName = table.Column<string>(type: "nvarchar(max)", nullable: false)
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
                });

            migrationBuilder.CreateIndex(
                name: "IX_CurrentClasses_SectionID",
                table: "CurrentClasses",
                column: "SectionID");

            migrationBuilder.CreateIndex(
                name: "IX_Sections_CourseID",
                table: "Sections",
                column: "CourseID");

            migrationBuilder.AddForeignKey(
                name: "FK_CurrentClasses_Sections_SectionID",
                table: "CurrentClasses",
                column: "SectionID",
                principalTable: "Sections",
                principalColumn: "SectionID");
        }
    }
}
