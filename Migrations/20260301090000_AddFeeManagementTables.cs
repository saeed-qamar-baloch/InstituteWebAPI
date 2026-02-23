using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InstituteWebAPI.Migrations
{
    [DbContext(typeof(InstituteWebAPI.Data.RozhnInstituteDbContext))]
    [Migration("20260301090000_AddFeeManagementTables")]
    public partial class AddFeeManagementTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FeeDues",
                columns: table => new
                {
                    FeeDueId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AdmissionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FeeType = table.Column<int>(type: "int", nullable: false),
                    FeeMonth = table.Column<DateTime>(type: "date", nullable: true),
                    BaseAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LateFeeAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DueDate = table.Column<DateTime>(type: "date", nullable: false),
                    IsLateFeeWaived = table.Column<bool>(type: "bit", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeeDues", x => x.FeeDueId);
                    table.ForeignKey(
                        name: "FK_FeeDues_Admissions_AdmissionId",
                        column: x => x.AdmissionId,
                        principalTable: "Admissions",
                        principalColumn: "AdmissionID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Payments",
                columns: table => new
                {
                    PaymentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PaymentDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PaymentMethod = table.Column<int>(type: "int", nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.PaymentId);
                    table.ForeignKey(
                        name: "FK_Payments_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "StudentID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PaymentDetails",
                columns: table => new
                {
                    PaymentDetailId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PaymentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FeeDueId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PaidAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentDetails", x => x.PaymentDetailId);
                    table.ForeignKey(
                        name: "FK_PaymentDetails_FeeDues_FeeDueId",
                        column: x => x.FeeDueId,
                        principalTable: "FeeDues",
                        principalColumn: "FeeDueId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PaymentDetails_Payments_PaymentId",
                        column: x => x.PaymentId,
                        principalTable: "Payments",
                        principalColumn: "PaymentId",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FeeDues_AdmissionId_FeeType_FeeMonth",
                table: "FeeDues",
                columns: new[] { "AdmissionId", "FeeType", "FeeMonth" },
                unique: true,
                filter: "[FeeMonth] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_FeeDues_AdmissionId",
                table: "FeeDues",
                column: "AdmissionId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentDetails_FeeDueId",
                table: "PaymentDetails",
                column: "FeeDueId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentDetails_PaymentId",
                table: "PaymentDetails",
                column: "PaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_StudentId",
                table: "Payments",
                column: "StudentId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PaymentDetails");

            migrationBuilder.DropTable(
                name: "FeeDues");

            migrationBuilder.DropTable(
                name: "Payments");
        }
    }
}
