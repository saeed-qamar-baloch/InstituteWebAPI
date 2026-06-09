using InstituteWebAPI.Models.DTO.Notifications;
using InstituteWebApp.Models.Domain;

namespace InstituteWebAPI.Services.Notifications
{
    public interface IAppNotificationService
    {
        /// <summary>Creates one notification per user in the given role.</summary>
        Task NotifyRoleAsync(string role, AppNotificationType type, string title, string message, string? link = null);

        /// <summary>Creates a notification for a single user.</summary>
        Task NotifyUserAsync(string userId, AppNotificationType type, string title, string message, string? link = null);

        Task<IReadOnlyList<AppNotificationDto>> GetForUserAsync(string userId, int take = 30);
        Task<int> GetUnreadCountAsync(string userId);
        Task<bool> MarkReadAsync(string userId, Guid id);
        Task<int> MarkAllReadAsync(string userId);
        Task<bool> DeleteAsync(string userId, Guid id);
    }
}
