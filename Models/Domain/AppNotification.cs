using System.ComponentModel.DataAnnotations;

namespace InstituteWebApp.Models.Domain
{
    public enum AppNotificationType
    {
        General = 0,
        LeaveRequest = 1,
        CardRequest = 2,
        MarkEditRequest = 3,
        MaterialRequest = 4,
        IssueReport     = 5,
    }

    /// <summary>
    /// An in-app notification shown in the navbar bell for a single user.
    /// One row per recipient (a role broadcast expands into one row per user).
    /// </summary>
    public class AppNotification
    {
        [Key]
        public Guid AppNotificationId { get; set; }

        /// <summary>IdentityUser ID of the recipient.</summary>
        [Required]
        public string UserId { get; set; } = string.Empty;

        public AppNotificationType Type { get; set; }

        [Required]
        [MaxLength(150)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        public string Message { get; set; } = string.Empty;

        /// <summary>Front-end route to open when the notification is clicked.</summary>
        [MaxLength(200)]
        public string? Link { get; set; }

        public bool IsRead { get; set; }

        public DateTime? ReadAt { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
