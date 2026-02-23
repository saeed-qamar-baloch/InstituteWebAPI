using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InstituteWebAPI.Migrations
{
    [DbContext(typeof(InstituteWebAPI.Data.RozhnInstituteDbContext))]
    [Migration("20260302090000_FixPaymentDetailIndex")]
    public partial class FixPaymentDetailIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_PaymentDetails_FeeDueId' AND object_id = OBJECT_ID('PaymentDetails'))
    DROP INDEX [IX_PaymentDetails_FeeDueId] ON [PaymentDetails];
CREATE INDEX [IX_PaymentDetails_FeeDueId] ON [PaymentDetails] ([FeeDueId]);
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_PaymentDetails_FeeDueId' AND object_id = OBJECT_ID('PaymentDetails'))
    DROP INDEX [IX_PaymentDetails_FeeDueId] ON [PaymentDetails];
CREATE UNIQUE INDEX [IX_PaymentDetails_FeeDueId] ON [PaymentDetails] ([FeeDueId]);
");
        }
    }
}
