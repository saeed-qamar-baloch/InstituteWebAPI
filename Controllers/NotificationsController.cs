using InstituteWebAPI.Data;
using InstituteWebAPI.Services.Sms;
using InstituteWebApp.Models.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace InstituteWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class NotificationsController : ControllerBase
    {
        private readonly RozhnInstituteDbContext dbContext;
        private readonly ISmsService smsService;

        public NotificationsController(RozhnInstituteDbContext dbContext, ISmsService smsService)
        {
            this.dbContext = dbContext;
            this.smsService = smsService;
        }

        // ── DTOs ─────────────────────────────────────────────────────────────

        public class CreateNotificationDto
        {
            public int NotificationType { get; set; }    // NotificationType enum
            public int RecipientType    { get; set; }    // NotificationRecipientType enum
            public Guid? RecipientID    { get; set; }    // null when RecipientType = All
            public int Channel          { get; set; }    // NotificationChannel enum
            public string Title         { get; set; } = string.Empty;
            public string Message       { get; set; } = string.Empty;
        }

        public class BroadcastNotificationDto
        {
            public int NotificationType { get; set; }
            public int Channel          { get; set; }
            public string Title         { get; set; } = string.Empty;
            public string Message       { get; set; } = string.Empty;
            /// <summary>
            /// 1 = Student, 2 = Teacher, 3 = All.
            /// Generates one Notification record per matching recipient.
            /// </summary>
            public int RecipientType    { get; set; } = (int)NotificationRecipientType.All;
        }

        // ── GET api/Notifications ─────────────────────────────────────────────
        // Optional filters: recipientId, recipientType, status, channel, type
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] Guid?   recipientId   = null,
            [FromQuery] int?    recipientType = null,
            [FromQuery] int?    status        = null,
            [FromQuery] int?    channel       = null,
            [FromQuery] int?    type          = null,
            [FromQuery] DateTime? fromDate    = null,
            [FromQuery] DateTime? toDate      = null)
        {
            var query = dbContext.Notifications.AsNoTracking().AsQueryable();

            if (recipientId.HasValue)
                query = query.Where(n => n.RecipientID == recipientId.Value);

            if (recipientType.HasValue)
                query = query.Where(n => (int)n.RecipientType == recipientType.Value);

            if (status.HasValue)
                query = query.Where(n => (int)n.Status == status.Value);

            if (channel.HasValue)
                query = query.Where(n => (int)n.Channel == channel.Value);

            if (type.HasValue)
                query = query.Where(n => (int)n.NotificationType == type.Value);

            if (fromDate.HasValue)
                query = query.Where(n => n.CreatedAt >= fromDate.Value.Date);

            if (toDate.HasValue)
                query = query.Where(n => n.CreatedAt <= toDate.Value.Date.AddDays(1).AddTicks(-1));

            var list = await query
                .OrderByDescending(n => n.CreatedAt)
                .Select(n => new
                {
                    n.NotificationID,
                    NotificationType = n.NotificationType.ToString(),
                    RecipientType    = n.RecipientType.ToString(),
                    n.RecipientID,
                    Channel          = n.Channel.ToString(),
                    n.Title,
                    n.Message,
                    Status           = n.Status.ToString(),
                    n.SentAt,
                    n.ErrorMessage,
                    n.CreatedByUserID,
                    n.CreatedAt
                })
                .ToListAsync();

            return Ok(list);
        }

        // ── GET api/Notifications/{id} ────────────────────────────────────────
        [HttpGet("{id:Guid}")]
        public async Task<IActionResult> GetById([FromRoute] Guid id)
        {
            var n = await dbContext.Notifications
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.NotificationID == id);

            if (n == null) return NotFound();

            return Ok(new
            {
                n.NotificationID,
                NotificationType = n.NotificationType.ToString(),
                RecipientType    = n.RecipientType.ToString(),
                n.RecipientID,
                Channel          = n.Channel.ToString(),
                n.Title,
                n.Message,
                Status           = n.Status.ToString(),
                n.SentAt,
                n.ErrorMessage,
                n.CreatedByUserID,
                n.CreatedAt
            });
        }

        // ── POST api/Notifications ────────────────────────────────────────────
        // Create a single notification record (status = Pending).
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateNotificationDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Title))   return BadRequest("Title is required.");
            if (string.IsNullOrWhiteSpace(dto.Message)) return BadRequest("Message is required.");

            if ((NotificationRecipientType)dto.RecipientType != NotificationRecipientType.All
                && dto.RecipientID == null)
                return BadRequest("RecipientID is required for non-broadcast notifications.");

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var notification = new Notification
            {
                NotificationID   = Guid.NewGuid(),
                NotificationType = (NotificationType)dto.NotificationType,
                RecipientType    = (NotificationRecipientType)dto.RecipientType,
                RecipientID      = dto.RecipientID,
                Channel          = (NotificationChannel)dto.Channel,
                Title            = dto.Title,
                Message          = dto.Message,
                Status           = NotificationStatus.Pending,
                CreatedByUserID  = userId,
                CreatedAt        = DateTime.UtcNow
            };

            dbContext.Notifications.Add(notification);
            await dbContext.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = notification.NotificationID }, new
            {
                notification.NotificationID,
                Status = notification.Status.ToString()
            });
        }

        // ── POST api/Notifications/{id}/send ─────────────────────────────────
        // Dispatch a single pending notification.
        // For InApp: marks Sent immediately.
        // For SMS / Email: stub — marks Sent (wire up a real provider here).
        [HttpPost("{id:Guid}/send")]
        public async Task<IActionResult> Send([FromRoute] Guid id)
        {
            var notification = await dbContext.Notifications
                .FirstOrDefaultAsync(n => n.NotificationID == id);

            if (notification == null) return NotFound();

            if (notification.Status == NotificationStatus.Sent)
                return BadRequest("Notification has already been sent.");

            try
            {
                await DispatchAsync(notification);
                notification.Status = NotificationStatus.Sent;
                notification.SentAt = DateTime.UtcNow;
                notification.ErrorMessage = null;
            }
            catch (Exception ex)
            {
                notification.Status       = NotificationStatus.Failed;
                notification.ErrorMessage = ex.Message;
            }

            await dbContext.SaveChangesAsync();

            return Ok(new
            {
                notification.NotificationID,
                Status = notification.Status.ToString(),
                notification.SentAt,
                notification.ErrorMessage
            });
        }

        // ── POST api/Notifications/broadcast ─────────────────────────────────
        // Creates one notification record per matching recipient and dispatches all.
        // RecipientType 1 = Students, 2 = Teachers, 3 = All (both).
        [HttpPost("broadcast")]
        public async Task<IActionResult> Broadcast([FromBody] BroadcastNotificationDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Title))   return BadRequest("Title is required.");
            if (string.IsNullOrWhiteSpace(dto.Message)) return BadRequest("Message is required.");

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var recipientType = (NotificationRecipientType)dto.RecipientType;

            var notifications = new List<Notification>();
            var now = DateTime.UtcNow;

            if (recipientType == NotificationRecipientType.All)
            {
                // Single broadcast record with no specific recipient
                notifications.Add(MakeNotification(dto, null, recipientType, userId, now));
            }
            else if (recipientType == NotificationRecipientType.Student)
            {
                var studentIds = await dbContext.Students
                    .AsNoTracking()
                    .Where(s => s.IsEnrolled)
                    .Select(s => s.StudentID)
                    .ToListAsync();

                notifications.AddRange(studentIds.Select(sid =>
                    MakeNotification(dto, sid, recipientType, userId, now)));
            }
            else if (recipientType == NotificationRecipientType.Teacher)
            {
                var teacherIds = await dbContext.Teachers
                    .AsNoTracking()
                    .Select(t => t.TeacherID)
                    .ToListAsync();

                notifications.AddRange(teacherIds.Select(tid =>
                    MakeNotification(dto, tid, recipientType, userId, now)));
            }

            if (notifications.Count == 0)
                return Ok(new { Message = "No recipients found.", Sent = 0, Failed = 0 });

            dbContext.Notifications.AddRange(notifications);
            await dbContext.SaveChangesAsync();

            // Dispatch
            int sent = 0, failed = 0;
            foreach (var n in notifications)
            {
                try
                {
                    await DispatchAsync(n);
                    n.Status = NotificationStatus.Sent;
                    n.SentAt = DateTime.UtcNow;
                    sent++;
                }
                catch (Exception ex)
                {
                    n.Status       = NotificationStatus.Failed;
                    n.ErrorMessage = ex.Message;
                    failed++;
                }
            }

            await dbContext.SaveChangesAsync();

            return Ok(new
            {
                TotalRecipients = notifications.Count,
                Sent            = sent,
                Failed          = failed
            });
        }

        // ── DELETE api/Notifications/{id} ─────────────────────────────────────
        [HttpDelete("{id:Guid}")]
        public async Task<IActionResult> Delete([FromRoute] Guid id)
        {
            var notification = await dbContext.Notifications.FindAsync(id);
            if (notification == null) return NotFound();

            dbContext.Notifications.Remove(notification);
            await dbContext.SaveChangesAsync();
            return NoContent();
        }

        // ── Private helpers ───────────────────────────────────────────────────

        private static Notification MakeNotification(
            BroadcastNotificationDto dto,
            Guid? recipientId,
            NotificationRecipientType recipientType,
            string? userId,
            DateTime now) => new()
        {
            NotificationID   = Guid.NewGuid(),
            NotificationType = (NotificationType)dto.NotificationType,
            RecipientType    = recipientType,
            RecipientID      = recipientId,
            Channel          = (NotificationChannel)dto.Channel,
            Title            = dto.Title,
            Message          = dto.Message,
            Status           = NotificationStatus.Pending,
            CreatedByUserID  = userId,
            CreatedAt        = now
        };

        /// <summary>
        /// Dispatch logic per channel. InApp is a no-op (record in DB is the
        /// notification). SMS is wired to ISmsService (local Pakistani gateway —
        /// see SmsGatewaySettings). Email is still stubbed — wire up a provider
        /// (e.g. SendGrid) when ready.
        /// </summary>
        private async Task DispatchAsync(Notification notification)
        {
            switch (notification.Channel)
            {
                case NotificationChannel.InApp:
                    return; // stored in DB — UI polls/reads

                case NotificationChannel.SMS:
                    var phone = await ResolveRecipientPhoneAsync(notification.RecipientType, notification.RecipientID);
                    await smsService.SendAsync(phone ?? string.Empty, notification.Message);
                    return;

                case NotificationChannel.Email:
                    return; // TODO: integrate email provider

                default:
                    return;
            }
        }

        /// <summary>Looks up the registered contact number for a notification recipient.
        /// Students: FatherContact first, falls back to StudentContact.
        /// Teachers: Contact.</summary>
        private async Task<string?> ResolveRecipientPhoneAsync(NotificationRecipientType recipientType, Guid? recipientId)
        {
            if (recipientId == null) return null;

            if (recipientType == NotificationRecipientType.Student)
            {
                var student = await dbContext.Students
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s => s.StudentID == recipientId.Value);

                if (student == null) return null;
                return !string.IsNullOrWhiteSpace(student.FatherContact) ? student.FatherContact : student.StudentContact;
            }

            if (recipientType == NotificationRecipientType.Teacher)
            {
                var teacher = await dbContext.Teachers
                    .AsNoTracking()
                    .FirstOrDefaultAsync(t => t.TeacherID == recipientId.Value);

                return teacher?.Contact;
            }

            return null;
        }

        // ── POST api/Notifications/sms/fee-reminders ─────────────────────────
        // Sends a fee-due reminder SMS to selected students (e.g. from the Fee
        // List page). One Notification row is created per student (so it shows
        // up in normal notification history/audit), then dispatched immediately.
        [HttpPost("sms/fee-reminders")]
        public async Task<IActionResult> SendFeeReminderSms([FromBody] SendFeeReminderSmsDto dto)
        {
            if (dto.StudentIDs == null || dto.StudentIDs.Count == 0)
                return BadRequest("No students selected.");

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var now = DateTime.UtcNow;

            var students = await dbContext.Students
                .AsNoTracking()
                .Where(s => dto.StudentIDs.Contains(s.StudentID))
                .ToListAsync();

            var notifications = new List<Notification>();
            var skippedNoPhone = new List<string>();

            foreach (var s in students)
            {
                var phone = !string.IsNullOrWhiteSpace(s.FatherContact) ? s.FatherContact : s.StudentContact;
                if (string.IsNullOrWhiteSpace(phone))
                {
                    skippedNoPhone.Add(s.StudentName);
                    continue;
                }

                var message = string.IsNullOrWhiteSpace(dto.MessageTemplate)
                    ? $"Dear Parent, fee dues for {s.StudentName} are outstanding. Please clear payment at your earliest convenience. - Rozhn Institute"
                    : dto.MessageTemplate.Replace("{StudentName}", s.StudentName);

                notifications.Add(new Notification
                {
                    NotificationID   = Guid.NewGuid(),
                    NotificationType = NotificationType.FeeReminder,
                    RecipientType    = NotificationRecipientType.Student,
                    RecipientID      = s.StudentID,
                    Channel          = NotificationChannel.SMS,
                    Title            = "Fee Reminder",
                    Message          = message,
                    Status           = NotificationStatus.Pending,
                    CreatedByUserID  = userId,
                    CreatedAt        = now
                });
            }

            if (notifications.Count == 0)
            {
                return Ok(new
                {
                    Message = "No valid phone numbers found for selected students.",
                    Sent = 0,
                    Failed = 0,
                    SkippedNoPhone = skippedNoPhone
                });
            }

            dbContext.Notifications.AddRange(notifications);
            await dbContext.SaveChangesAsync();

            int sent = 0, failed = 0;
            var failures = new List<object>();

            foreach (var n in notifications)
            {
                try
                {
                    await DispatchAsync(n);
                    n.Status = NotificationStatus.Sent;
                    n.SentAt = DateTime.UtcNow;
                    n.ErrorMessage = null;
                    sent++;
                }
                catch (Exception ex)
                {
                    n.Status = NotificationStatus.Failed;
                    n.ErrorMessage = ex.Message;
                    failed++;
                    failures.Add(new { n.RecipientID, ex.Message });
                }
            }

            await dbContext.SaveChangesAsync();

            return Ok(new
            {
                TotalRecipients = notifications.Count,
                Sent = sent,
                Failed = failed,
                SkippedNoPhone = skippedNoPhone,
                Failures = failures
            });
        }

        public class SendFeeReminderSmsDto
        {
            public List<Guid> StudentIDs { get; set; } = new();
            /// <summary>Optional override. Use {StudentName} as a placeholder.
            /// Falls back to a default fee-reminder message when omitted.</summary>
            public string? MessageTemplate { get; set; }
        }
    }
}
