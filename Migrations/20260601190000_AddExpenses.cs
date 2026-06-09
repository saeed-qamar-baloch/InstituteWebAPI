using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InstituteWebAPI.Migrations
{
    [DbContext(typeof(InstituteWebAPI.Data.RozhnInstituteDbContext))]
    [Migration("20260601190000_AddExpenses")]
    public partial class AddExpenses : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ExpenseCategories",
                columns: table => new
                {
                    ExpenseCategoryID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExpenseCategories", x => x.ExpenseCategoryID);
                });

            migrationBuilder.CreateTable(
                name: "Expenses",
                columns: table => new
                {
                    ExpenseID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExpenseCategoryID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ExpenseDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PaymentMethod = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Expenses", x => x.ExpenseID);
                    table.ForeignKey(
                        name: "FK_Expenses_ExpenseCategories_ExpenseCategoryID",
                        column: x => x.ExpenseCategoryID,
                        principalTable: "ExpenseCategories",
                        principalColumn: "ExpenseCategoryID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Expenses_ExpenseCategoryID",
                table: "Expenses",
                column: "ExpenseCategoryID");

            // Seed a few default categories (raw SQL — no model dependency)
            void Seed(string id, string name, string desc) => migrationBuilder.Sql(
                $"INSERT INTO ExpenseCategories (ExpenseCategoryID, Name, Description, IsActive, CreatedOn) " +
                $"VALUES ('{id}', N'{name}', N'{desc}', 1, '2026-01-01');");

            Seed("e2000000-0000-0000-0000-000000000001", "Salaries",    "Staff and teacher salaries");
            Seed("e2000000-0000-0000-0000-000000000002", "Rent",        "Building / premises rent");
            Seed("e2000000-0000-0000-0000-000000000003", "Utilities",   "Electricity, water, internet");
            Seed("e2000000-0000-0000-0000-000000000004", "Stationery",  "Books, paper, supplies");
            Seed("e2000000-0000-0000-0000-000000000005", "Maintenance", "Repairs and upkeep");
            Seed("e2000000-0000-0000-0000-000000000006", "Misc",        "Miscellaneous expenses");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "Expenses");
            migrationBuilder.DropTable(name: "ExpenseCategories");
        }
    }
}
