using InstituteWebAPI.Services.Notifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace InstituteWebAPI.Controllers
{
    // In-app notification feed for the currently logged-in user (any role).
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AppNotificationsController : ControllerBase
    {
        private readonly IAppNotificationService service;

        public AppNotificationsController(IAppNotificationService service)
        {
            this.service = service;
        }

        private string? CurrentUserId =>
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        // GET api/AppNotifications?take=30
        [HttpGet]
        public async Task<IActionResult> GetMine([FromQuery] int take = 30)
        {
            var userId = CurrentUserId;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();
            var list = await service.GetForUserAsync(userId, take);
            return Ok(list);
        }

        // GET api/AppNotifications/unread-count
        [HttpGet("unread-count")]
        public async Task<IActionResult> UnreadCount()
        {
            var userId = CurrentUserId;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();
            var count = await service.GetUnreadCountAsync(userId);
            return Ok(new { count });
        }

        // POST api/AppNotifications/{id}/read
        [HttpPost("{id:Guid}/read")]
        public async Task<IActionResult> MarkRead([FromRoute] Guid id)
        {
            var userId = CurrentUserId;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();
            var ok = await service.MarkReadAsync(userId, id);
            return ok ? Ok() : NotFound();
        }

        // POST api/AppNotifications/read-all
        [HttpPost("read-all")]
        public async Task<IActionResult> MarkAllRead()
        {
            var userId = CurrentUserId;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();
            var n = await service.MarkAllReadAsync(userId);
            return Ok(new { marked = n });
        }

        // DELETE api/AppNotifications/{id}
        [HttpDelete("{id:Guid}")]
        public async Task<IActionResult> Delete([FromRoute] Guid id)
        {
            var userId = CurrentUserId;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();
            var ok = await service.DeleteAsync(userId, id);
            return ok ? NoContent() : NotFound();
        }
    }
}
