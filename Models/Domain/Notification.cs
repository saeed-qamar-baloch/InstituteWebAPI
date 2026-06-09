using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InstituteWebApp.Models.Domain
{
    public enum NotificationChannel
    {
        InApp = 1,
        SMS = 2,
        Email = 3
    }

    public enum NotificationStatus
    {
        Pending = 1,
        Sent = 2,
        Failed = 3
    }

    public enum NotificationRecipientType
    {
        Student = 1,
        Teacher = 2,
        All = 3
    }

    public enum NotificationType
    {
        FeeReminder = 1,
        ResultAlert = 2,
        PromotionNotice = 3,
        General = 4
    }

    /// <summary>
    /// Stores outbound notifications (fee reminders, result alerts, promotion notices).
    /// Each row is one notification attempt to one recipient via one channel.
    /// RecipientID = null when RecipientType = All.
    /// </summary>
    public class Notification
    {
        [Key]
        public Guid NotificationID { get; set; }

        public NotificationType NotificationType { get; set; }

        public NotificationRecipientType RecipientType { get; set; }

        /// <summary>StudentID or TeacherID. Null when RecipientType = All.</summary>
        public Guid? RecipientID { get; set; }

        public NotificationChannel Channel { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string Message { get; set; }

        public NotificationStatus Status { get; set; } = NotificationStatus.Pending;

        public DateTime? SentAt { get; set; }

        /// <summary>Error detail if Status = Failed.</summary>
        public string? ErrorMessage { get; set; }

        /// <summary>IdentityUser ID of the admin who triggered this notification.</summary>
        public string? CreatedByUserID { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
