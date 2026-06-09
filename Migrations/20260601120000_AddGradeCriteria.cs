using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InstituteWebAPI.Migrations
{
    public partial class AddGradeCriteria : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GradeCriterias",
                columns: table => new
                {
                    GradeCriteriaID  = table.Column<Guid>(nullable: false),
                    GradeLabel       = table.Column<string>(maxLength: 10, nullable: false),
                    MinPercentage    = table.Column<float>(nullable: false),
                    Description      = table.Column<string>(maxLength: 50, nullable: true),
                    DisplayOrder     = table.Column<int>(nullable: false)
                },
                constraints: table => table.PrimaryKey("PK_GradeCriterias", x => x.GradeCriteriaID));

            // Seed defaults
            migrationBuilder.InsertData("GradeCriterias",
                new[] { "GradeCriteriaID","GradeLabel","MinPercentage","Description","DisplayOrder" },
                new object[,]
                {
                    { "a1000000-0000-0000-0000-000000000001", "A", 80f, "Excellent",     1 },
                    { "a1000000-0000-0000-0000-000000000002", "B", 70f, "Very Good",     2 },
                    { "a1000000-0000-0000-0000-000000000003", "C", 60f, "Good",          3 },
                    { "a1000000-0000-0000-0000-000000000004", "D", 50f, "Pass",          4 },
                    { "a1000000-0000-0000-0000-000000000005", "E", 45f, "Marginal Pass", 5 },
                    { "a1000000-0000-0000-0000-000000000006", "F",  0f, "Fail",          6 },
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable("GradeCriterias");
        }
    }
}
