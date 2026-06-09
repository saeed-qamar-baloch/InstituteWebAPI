using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InstituteWebAPI.Migrations
{
    [DbContext(typeof(InstituteWebAPI.Data.RozhnInstituteDbContext))]
    [Migration("20260601140000_AddClassRankAndManualResult")]
    public partial class AddClassRankAndManualResult : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Progression order within a course (1 = lowest). 0 = unranked.
            migrationBuilder.AddColumn<int>(
                name: "Rank",
                table: "Classes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            // Marks a terminal result whose Result was set manually by an admin,
            // so regeneration won't overwrite it.
            migrationBuilder.AddColumn<bool>(
                name: "IsResultManual",
                table: "TerminalResults",
                type: "bit",
                nullable: false,
                defaultValue: false);

            // Best-effort seed of common level ordering for existing classes.
            // Matches on the class name (case-insensitive via SQL collation).
            void SeedRank(string name, int rank) =>
                migrationBuilder.Sql(
                    $"UPDATE Classes SET Rank = {rank} WHERE LTRIM(RTRIM(ClassName)) = '{name}' AND Rank = 0;");

            SeedRank("Basic A", 1);
            SeedRank("Basic B", 2);
            SeedRank("Foundation", 3);
            SeedRank("Beginner", 4);
            SeedRank("Level 1", 5);
            SeedRank("Level 2", 6);
            SeedRank("Level 3", 7);
            SeedRank("Level 4", 8);
            SeedRank("Level 5", 9);
            SeedRank("Level 6", 10);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "Rank", table: "Classes");
            migrationBuilder.DropColumn(name: "IsResultManual", table: "TerminalResults");
        }
    }
}
