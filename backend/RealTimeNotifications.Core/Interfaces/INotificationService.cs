using RealTimeNotifications.Core.Models;

namespace RealTimeNotifications.Core.Interfaces
{
    public interface INotificationService
    {
        Task SendNotificationAsync(Notification notification);
        Task SendNotificationToUserAsync(string userId, Notification notification);
        Task SendNotificationToGroupAsync(string groupName, Notification notification);
        Task<IEnumerable<Notification>> GetUserNotificationsAsync(string userId);
        Task MarkAsReadAsync(string notificationId);
        Task MarkAllAsReadAsync(string userId);
    }
}
