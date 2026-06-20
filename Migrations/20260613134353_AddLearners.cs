using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable
/// <inheritdoc />
public partial class AddLearners : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Learners",
            schema: "web",
            columns: table => new
            {
                LearnerID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                DisplayName = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                GoogleSubject = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                CompletedLessonsJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                LearningDaysJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                CurrentStreak = table.Column<int>(type: "int", nullable: false),
                LongestStreak = table.Column<int>(type: "int", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                LastActiveDate = table.Column<DateTime>(type: "datetime2", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Learners", x => x.LearnerID);
            });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "Learners",
            schema: "web");
    }
}
