using InstituteWebAPI.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InstituteWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminBackupController : ControllerBase
    {
        private readonly RozhnInstituteDbContext dbContext;
        public AdminBackupController(RozhnInstituteDbContext dbContext) { this.dbContext = dbContext; }

        // Tables we never export (identity/security + migration bookkeeping)
        private static readonly HashSet<string> Excluded = new(StringComparer.OrdinalIgnoreCase)
        {
            "__EFMigrationsHistory",
            "AspNetUsers", "AspNetRoles", "AspNetUserRoles", "AspNetUserClaims",
            "AspNetUserLogins", "AspNetUserTokens", "AspNetRoleClaims",
        };

        // ── GET api/AdminBackup ──────────────────────────────────────────────
        // Full data export: { generatedAt, tables: { TableName: [ {col:val,...}, ... ] } }
        [HttpGet]
        public async Task<IActionResult> Export()
        {
            var conn = dbContext.Database.GetDbConnection();
            var opened = false;
            if (conn.State != System.Data.ConnectionState.Open) { await conn.OpenAsync(); opened = true; }
            try
            {
                // List base tables
                var tables = new List<string>();
                await using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE' ORDER BY TABLE_NAME";
                    await using var rd = await cmd.ExecuteReaderAsync();
                    while (await rd.ReadAsync()) tables.Add(rd.GetString(0));
                }

                var data = new Dictionary<string, List<Dictionary<string, object?>>>();
                long totalRows = 0;

                foreach (var t in tables.Where(t => !Excluded.Contains(t)))
                {
                    var rows = new List<Dictionary<string, object?>>();
                    await using var cmd = conn.CreateCommand();
                    cmd.CommandText = $"SELECT * FROM [{t}]";
                    await using var rd = await cmd.ExecuteReaderAsync();
                    while (await rd.ReadAsync())
                    {
                        var row = new Dictionary<string, object?>(rd.FieldCount);
                        for (int i = 0; i < rd.FieldCount; i++)
                            row[rd.GetName(i)] = await rd.IsDBNullAsync(i) ? null : rd.GetValue(i);
                        rows.Add(row);
                    }
                    data[t] = rows;
                    totalRows += rows.Count;
                }

                return Ok(new
                {
                    GeneratedAt = DateTime.UtcNow,
                    TableCount = data.Count,
                    TotalRows = totalRows,
                    Tables = data,
                });
            }
            finally
            {
                if (opened) await conn.CloseAsync();
            }
        }
    }
}
