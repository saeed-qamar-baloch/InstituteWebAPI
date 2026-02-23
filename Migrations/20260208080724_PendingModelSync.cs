using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InstituteWebAPI.Migrations
{
    /// <inheritdoc />
    public partial class PendingModelSync : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FeeCollectionDetails");

            migrationBuilder.DropTable(
                name: "FeeCollections");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FeeCollections",
                columns: table => new
                {
                    FeeCollectionID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudentID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PaymentDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeeCollections", x => x.FeeCollectionID);
                    table.ForeignKey(
                        name: "FK_FeeCollections_Students_StudentID",
                        column: x => x.StudentID,
                        principalTable: "Students",
                        principalColumn: "StudentID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FeeCollectionDetails",
                columns: table => new
                {
                    FeeCollectionDetailID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FeeCollectionID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FeeTypeID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Month = table.Column<int>(type: "int", nullable: true),
                    Year = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeeCollectionDetails", x => x.FeeCollectionDetailID);
                    table.ForeignKey(
                        name: "FK_FeeCollectionDetails_FeeCollections_FeeCollectionID",
                        column: x => x.FeeCollectionID,
                        principalTable: "FeeCollections",
                        principalColumn: "FeeCollectionID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FeeCollectionDetails_FeeTypes_FeeTypeID",
                        column: x => x.FeeTypeID,
                        principalTable: "FeeTypes",
                        principalColumn: "FeeTypeID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FeeCollectionDetails_FeeCollectionID_FeeTypeID_Month_Year",
                table: "FeeCollectionDetails",
                columns: new[] { "FeeCollectionID", "FeeTypeID", "Month", "Year" },
                unique: true,
                filter: "[Month] IS NOT NULL AND [Year] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_FeeCollectionDetails_FeeTypeID",
                table: "FeeCollectionDetails",
                column: "FeeTypeID");

            migrationBuilder.CreateIndex(
                name: "IX_FeeCollections_StudentID",
                table: "FeeCollections",
                column: "StudentID");
        }
    }
}
