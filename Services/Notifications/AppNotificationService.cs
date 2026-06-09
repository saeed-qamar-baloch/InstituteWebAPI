using InstituteWebAPI.Data;
using InstituteWebAPI.Models.DTO.Notifications;
using InstituteWebApp.Models.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace InstituteWebAPI.Services.Notifications
{
    public class AppNotificationService : IAppNotificationService
    {
        private readonly RozhnInstituteDbContext db;
        private readonly UserManager<IdentityUser> userManager;

        public AppNotificationService(RozhnInstituteDbContext db, UserManager<IdentityUser> userManager)
        {
            this.db = db;
            this.userManager = userManager;
        }

        public async Task NotifyRoleAsync(string role, AppNotificationType type, string title, string message, string? link = null)
        {
            var users = await userManager.GetUsersInRoleAsync(role);
            if (users.Count == 0) return;

            var now = DateTime.UtcNow;
            var rows = users.Select(u => new AppNotification
            {
                AppNotificationId = Guid.NewGuid(),
                UserId    = u.Id,
                Type      = type,
                Title     = title,
                Message   = message,
                Link      = link,
                IsRead    = false,
                CreatedAt = now
            });

            db.AppNotifications.AddRange(rows);
            await db.SaveChangesAsync();
        }

        public async Task NotifyUserAsync(string userId, AppNotificationType type, string title, string message, string? link = null)
        {
            if (string.IsNullOrWhiteSpace(userId)) return;

            db.AppNotifications.Add(new AppNotification
            {
                AppNotificationId = Guid.NewGuid(),
                UserId    = userId,
                Type      = type,
                Title     = title,
                Message   = message,
                Link      = link,
                IsRead    = false,
                CreatedAt = DateTime.UtcNow
            });
            await db.SaveChangesAsync();
        }

        public async Task<IReadOnlyList<AppNotificationDto>> GetForUserAsync(string userId, int take = 30)
        {
            return await db.AppNotifications
                .AsNoTracking()
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Take(take)
                .Select(n => new AppNotificationDto
                {
                    AppNotificationId = n.AppNotificationId,
                    Type      = n.Type.ToString(),
                    Title     = n.Title,
                    Message   = n.Message,
                    Link      = n.Link,
                    IsRead    = n.IsRead,
                    ReadAt    = n.ReadAt,
                    CreatedAt = n.CreatedAt
                })
                .ToListAsync();
        }

        public async Task<int> GetUnreadCountAsync(string userId)
        {
            return await db.AppNotifications
                .CountAsync(n => n.UserId == userId && !n.IsRead);
        }

        public async Task<bool> MarkReadAsync(string userId, Guid id)
        {
            var n = await db.AppNotifications
                .FirstOrDefaultAsync(x => x.AppNotificationId == id && x.UserId == userId);
            if (n == null) return false;
            if (!n.IsRead)
            {
                n.IsRead = true;
                n.ReadAt = DateTime.UtcNow;
                await db.SaveChangesAsync();
            }
            return true;
        }

        public async Task<int> MarkAllReadAsync(string userId)
        {
            var unread = await db.AppNotifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync();
            var now = DateTime.UtcNow;
            foreach (var n in unread) { n.IsRead = true; n.ReadAt = now; }
            if (unread.Count > 0) await db.SaveChangesAsync();
            return unread.Count;
        }

        public async Task<bool> DeleteAsync(string userId, Guid id)
        {
            var n = await db.AppNotifications
                .FirstOrDefaultAsync(x => x.AppNotificationId == id && x.UserId == userId);
            if (n == null) return false;
            db.AppNotifications.Remove(n);
            await db.SaveChangesAsync();
            return true;
        }
    }
}
