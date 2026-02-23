using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InstituteWebAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddFeePayments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StudentFees");

            migrationBuilder.CreateTable(
                name: "FeePayments",
                columns: table => new
                {
                    FeePaymentID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudentID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FeeTypeID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PaymentMonth = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LateFee = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PaidAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeePayments", x => x.FeePaymentID);
                    table.ForeignKey(
                        name: "FK_FeePayments_FeeTypes_FeeTypeID",
                        column: x => x.FeeTypeID,
                        principalTable: "FeeTypes",
                        principalColumn: "FeeTypeID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FeePayments_Students_StudentID",
                        column: x => x.StudentID,
                        principalTable: "Students",
                        principalColumn: "StudentID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FeePayments_FeeTypeID",
                table: "FeePayments",
                column: "FeeTypeID");

            migrationBuilder.CreateIndex(
                name: "IX_FeePayments_StudentID_FeeTypeID_PaymentMonth",
                table: "FeePayments",
                columns: new[] { "StudentID", "FeeTypeID", "PaymentMonth" },
                unique: true,
                filter: "[PaymentMonth] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FeePayments");

            migrationBuilder.CreateTable(
                name: "StudentFees",
                columns: table => new
                {
                    StudentFeeID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AdmissionID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FeeTypeID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LateFee = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Month = table.Column<int>(type: "int", nullable: true),
                    MonthlyFee = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PaidAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    StudentID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentFees", x => x.StudentFeeID);
                });
        }
    }
}
