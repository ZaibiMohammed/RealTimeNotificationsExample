using Microsoft.AspNetCore.Mvc;
using RealTimeNotifications.Core.Interfaces;
using RealTimeNotifications.Core.Models;

namespace RealTimeNotifications.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationsController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpPost("broadcast")]
        public async Task<IActionResult> BroadcastNotification([FromBody] Notification notification)
        {
            await _notificationService.SendNotificationAsync(notification);
            return Ok();
        }

        [HttpPost("users/{userId}")]
        public async Task<IActionResult> SendToUser(string userId, [FromBody] Notification notification)
        {
            await _notificationService.SendNotificationToUserAsync(userId, notification);
            return Ok();
        }

        [HttpPost("groups/{groupName}")]
        public async Task<IActionResult> SendToGroup(string groupName, [FromBody] Notification notification)
        {
            await _notificationService.SendNotificationToGroupAsync(groupName, notification);
            return Ok();
        }

        [HttpGet("users/{userId}")]
        public async Task<IActionResult> GetUserNotifications(string userId)
        {
            var notifications = await _notificationService.GetUserNotificationsAsync(userId);
            return Ok(notifications);
        }

        [HttpPut("{notificationId}/read")]
        public async Task<IActionResult> MarkAsRead(string notificationId)
        {
            await _notificationService.MarkAsReadAsync(notificationId);
            return Ok();
        }

        [HttpPut("users/{userId}/read-all")]
        public async Task<IActionResult> MarkAllAsRead(string userId)
        {
            await _notificationService.MarkAllAsReadAsync(userId);
            return Ok();
        }
    }
}
