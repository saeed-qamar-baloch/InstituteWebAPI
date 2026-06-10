using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InstituteWebAPI.Migrations
{
    /// <summary>
    /// Adds two columns that were previously applied via raw SQL at startup
    /// in Program.cs (now removed).  Both columns exist on the domain model
    /// but were never backed by a proper migration:
    ///
    ///   • StudentMonthlyResults.Status  — nullable nvarchar(50) status note
    ///   • CardRequests.RequestedByTeacherID — nullable FK to Teachers
    ///
    /// Safe to run on databases that already have these columns (IF NOT EXISTS
    /// semantics are provided by the raw-SQL guard that was in place before,
    /// so existing prod databases simply have these migration rows inserted
    /// without re-adding the columns — see the Up() implementation).
    /// </summary>
    [DbContext(typeof(InstituteWebAPI.Data.RozhnInstituteDbContext))]
    [Migration("20260610000100_AddMissingColumns")]
    public partial class AddMissingColumns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ── Idempotency note ──────────────────────────────────────────────
            // These columns were previously added via raw SQL at startup (now
            // removed from Program.cs).  Any existing database already has them.
            // We use IF NOT EXISTS guards so this migration is safe to run on
            // both fresh databases and upgraded ones.

            // ── 1. StudentMonthlyResults.Status ──────────────────────────────
            migrationBuilder.Sql(@"
                IF NOT EXISTS (
                    SELECT 1 FROM sys.columns
                    WHERE object_id = OBJECT_ID(N'StudentMonthlyResults')
                      AND name = N'Status')
                BEGIN
                    ALTER TABLE [StudentMonthlyResults]
                        ADD [Status] nvarchar(50) NULL;
                END");

            // ── 2. CardRequests.RequestedByTeacherID ─────────────────────────
            migrationBuilder.Sql(@"
                IF NOT EXISTS (
                    SELECT 1 FROM sys.columns
                    WHERE object_id = OBJECT_ID(N'CardRequests')
                      AND name = N'RequestedByTeacherID')
                BEGIN
                    ALTER TABLE [CardRequests]
                        ADD [RequestedByTeacherID] uniqueidentifier NULL;
                END");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (
                    SELECT 1 FROM sys.indexes
                    WHERE object_id = OBJECT_ID(N'CardRequests')
                      AND name = N'IX_CardRequests_RequestedByTeacherID')
                BEGIN
                    CREATE INDEX [IX_CardRequests_RequestedByTeacherID]
                        ON [CardRequests] ([RequestedByTeacherID]);
                END");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (
                    SELECT 1 FROM sys.foreign_keys
                    WHERE parent_object_id = OBJECT_ID(N'CardRequests')
                      AND name = N'FK_CardRequests_Teachers_RequestedByTeacherID')
                BEGIN
                    ALTER TABLE [CardRequests]
                        ADD CONSTRAINT [FK_CardRequests_Teachers_RequestedByTeacherID]
                        FOREIGN KEY ([RequestedByTeacherID])
                        REFERENCES [Teachers] ([TeacherID]);
                END");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CardRequests_Teachers_RequestedByTeacherID",
                table: "CardRequests");

            migrationBuilder.DropIndex(
                name: "IX_CardRequests_RequestedByTeacherID",
                table: "CardRequests");

            migrationBuilder.DropColumn(
                name: "RequestedByTeacherID",
                table: "CardRequests");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "StudentMonthlyResults");
        }
    }
}
