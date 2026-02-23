using System;
using InstituteWebAPI.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InstituteWebAPI.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(RozhnInstituteDbContext))]
    [Migration("20260312093000_AddSectionTermId")]
    public partial class AddSectionTermId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF COL_LENGTH('Sections', 'TermID') IS NULL
BEGIN
    ALTER TABLE [Sections] ADD [TermID] uniqueidentifier NULL;
END

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Sections_TermID' AND object_id = OBJECT_ID('Sections'))
BEGIN
    CREATE INDEX [IX_Sections_TermID] ON [Sections] ([TermID]);
END

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Sections_Term_TermID')
BEGIN
    ALTER TABLE [Sections] ADD CONSTRAINT [FK_Sections_Term_TermID] FOREIGN KEY ([TermID]) REFERENCES [Term] ([TermID]);
END
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Sections_Term_TermID')
BEGIN
    ALTER TABLE [Sections] DROP CONSTRAINT [FK_Sections_Term_TermID];
END

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Sections_TermID' AND object_id = OBJECT_ID('Sections'))
BEGIN
    DROP INDEX [IX_Sections_TermID] ON [Sections];
END

IF COL_LENGTH('Sections', 'TermID') IS NOT NULL
BEGIN
    ALTER TABLE [Sections] DROP COLUMN [TermID];
END
");
        }
    }
}
