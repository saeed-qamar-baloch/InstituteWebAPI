using InstituteWebAPI.Data;
using InstituteWebApp.Models.Domain;
using System.Security.Claims;

namespace InstituteWebAPI.Services.Audit
{
    public class AuditService : IAuditService
    {
        private readonly RozhnInstituteDbContext dbContext;
        private readonly IHttpContextAccessor http;

        public AuditService(RozhnInstituteDbContext dbContext, IHttpContextAccessor http)
        {
            this.dbContext = dbContext;
            this.http = http;
        }

        public async Task LogAsync(string module, string action, string? details = null, string? entityType = null, string? entityId = null)
        {
            try
            {
                var user = http.HttpContext?.User;
                var name = user?.FindFirstValue(ClaimTypes.Name)
                           ?? user?.Identity?.Name
                           ?? user?.FindFirstValue("name");
                var userId = user?.FindFirstValue(ClaimTypes.NameIdentifier);
                var role = user?.FindFirstValue(ClaimTypes.Role);

                dbContext.AuditLogs.Add(new AuditLog
                {
                    AuditLogID = Guid.NewGuid(),
                    UserName = name,
                    UserId = userId,
                    Role = role,
                    Module = module,
                    Action = action,
                    Details = details,
                    EntityType = entityType,
                    EntityId = entityId,
                    CreatedOn = DateTime.UtcNow,
                });
                await dbContext.SaveChangesAsync();
            }
            catch
            {
                // Auditing must never break the primary operation.
            }
        }
    }
}
