using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional
/// <inheritdoc />
public partial class AddWebsitePosts : Migration
{
    // NOTE: This migration was trimmed to ONLY create the WebsitePosts table.
    // The scaffolder had bundled unrelated drift operations (GradeCriterias,
    // Teachers.Email/Skills, index/FK changes) that belong to other pending
    // hand-written migrations and would have double-applied.

    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema(name: "web");

        migrationBuilder.CreateTable(
            name: "WebsitePosts",
            schema: "web",
            columns: table => new
            {
                WebsitePostID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                Body = table.Column<string>(type: "nvarchar(max)", nullable: false),
                PostType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                IsPublished = table.Column<bool>(type: "bit", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_WebsitePosts", x => x.WebsitePostID);
            });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "WebsitePosts",
            schema: "web");
    }
}
