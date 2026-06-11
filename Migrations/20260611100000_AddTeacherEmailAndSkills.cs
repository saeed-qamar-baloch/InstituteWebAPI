using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using InstituteWebAPI.Data;

#nullable disable

[Migration("20260611100000_AddTeacherEmailAndSkills")]
[DbContext(typeof(RozhnInstituteDbContext))]
partial class AddTeacherEmailAndSkills : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "Email",
            table: "Teachers",
            type: "nvarchar(max)",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "Skills",
            table: "Teachers",
            type: "nvarchar(max)",
            nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "Email", table: "Teachers");
        migrationBuilder.DropColumn(name: "Skills", table: "Teachers");
    }
}
