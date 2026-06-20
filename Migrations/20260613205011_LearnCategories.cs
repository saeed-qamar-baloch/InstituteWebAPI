using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable
/// <inheritdoc />
public partial class LearnCategories : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "Category",
            schema: "web",
            table: "Lessons",
            type: "nvarchar(60)",
            maxLength: 60,
            nullable: false,
            defaultValue: "");

        migrationBuilder.AddColumn<bool>(
            name: "IsPopular",
            schema: "web",
            table: "Lessons",
            type: "bit",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<bool>(
            name: "IsPractice",
            schema: "web",
            table: "Lessons",
            type: "bit",
            nullable: false,
            defaultValue: false);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "Category",
            schema: "web",
            table: "Lessons");

        migrationBuilder.DropColumn(
            name: "IsPopular",
            schema: "web",
            table: "Lessons");

        migrationBuilder.DropColumn(
            name: "IsPractice",
            schema: "web",
            table: "Lessons");
    }
}
