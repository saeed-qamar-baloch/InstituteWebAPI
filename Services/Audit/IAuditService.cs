namespace InstituteWebAPI.Services.Audit
{
    public interface IAuditService
    {
        Task LogAsync(string module, string action, string? details = null, string? entityType = null, string? entityId = null);
    }
}
