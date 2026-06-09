using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InstituteWebAPI.Migrations
{
    [DbContext(typeof(InstituteWebAPI.Data.RozhnInstituteDbContext))]
    [Migration("20260601250000_TeacherRegNoAndIdentity")]
    public partial class TeacherRegNoAndIdentity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "IdentityUserId",
                table: "Teachers",
                type: "nvarchar(max)",
                nullable: true);

            // 1) For teachers already linked to a login account, RegistrationNo currently
            //    holds the Identity user id (a GUID). Move it into IdentityUserId.
            migrationBuilder.Sql(@"
                UPDATE Teachers
                SET IdentityUserId = RegistrationNo
                WHERE RegistrationNo IS NOT NULL
                  AND LEN(RegistrationNo) = 36
                  AND RegistrationNo LIKE '%-%-%-%-%';");

            // 2) Regenerate every teacher's RegistrationNo to LT-{MMMyy}-{serial}
            //    using a fresh global serial ordered by existing serial / date.
            migrationBuilder.Sql(@"
                ;WITH cte AS (
                    SELECT TeacherID, RegistrationDate,
                           ROW_NUMBER() OVER (ORDER BY Serial, RegistrationDate, TeacherID) AS rn
                    FROM Teachers
                )
                UPDATE t
                SET t.Serial = c.rn,
                    t.RegistrationNo = 'LT-' + FORMAT(t.RegistrationDate, 'MMMyy', 'en-US')
                                       + '-' + RIGHT('000' + CAST(c.rn AS varchar(10)), 3)
                FROM Teachers t
                INNER JOIN cte c ON t.TeacherID = c.TeacherID;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Restore the GUID into RegistrationNo for linked teachers, then drop the column.
            migrationBuilder.Sql(@"
                UPDATE Teachers
                SET RegistrationNo = IdentityUserId
                WHERE IdentityUserId IS NOT NULL;");

            migrationBuilder.DropColumn(name: "IdentityUserId", table: "Teachers");
        }
    }
}
