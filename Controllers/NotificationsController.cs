using InstituteWebAPI.Data;
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

        public NotificationsController(RozhnInstituteDbContext dbContext)
        {
            this.dbContext = dbContext;
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
        /// notification). SMS / Email are stubbed — replace with your provider
        /// (e.g. Twilio, SendGrid) when ready.
        /// </summary>
        private static Task DispatchAsync(Notification notification)
        {
            return notification.Channel switch
            {
                NotificationChannel.InApp  => Task.CompletedTask,   // stored in DB — UI polls/reads
                NotificationChannel.SMS    => Task.CompletedTask,   // TODO: integrate SMS provider
                NotificationChannel.Email  => Task.CompletedTask,   // TODO: integrate email provider
                _                          => Task.CompletedTask
            };
        }
    }
}
