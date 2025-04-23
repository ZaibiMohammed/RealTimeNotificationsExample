using Microsoft.AspNetCore.SignalR;
using RealTimeNotifications.Core.Interfaces;
using RealTimeNotifications.Core.Models;
using RealTimeNotifications.Infrastructure.Hubs;
using System.Collections.Concurrent;

namespace RealTimeNotifications.Infrastructure.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IHubContext<NotificationHub> _hubContext;
        private static readonly ConcurrentDictionary<string, List<Notification>> _userNotifications = new();

        public NotificationService(IHubContext<NotificationHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task SendNotificationAsync(Notification notification)
        {
            await _hubContext.Clients.All.SendAsync("ReceiveNotification", notification);
        }

        public async Task SendNotificationToUserAsync(string userId, Notification notification)
        {
            notification.UserId = userId;
            StoreNotification(notification);
            
            await _hubContext.Clients.Group(userId).SendAsync("ReceiveNotification", notification);
        }

        public async Task SendNotificationToGroupAsync(string groupName, Notification notification)
        {
            await _hubContext.Clients.Group(groupName).SendAsync("ReceiveNotification", notification);
        }

        public Task<IEnumerable<Notification>> GetUserNotificationsAsync(string userId)
        {
            if (_userNotifications.TryGetValue(userId, out var notifications))
            {
                return Task.FromResult(notifications.OrderByDescending(n => n.CreatedAt).AsEnumerable());
            }
            
            return Task.FromResult(Enumerable.Empty<Notification>());
        }

        public Task MarkAsReadAsync(string notificationId)
        {
            foreach (var userNotifications in _userNotifications.Values)
            {
                var notification = userNotifications.FirstOrDefault(n => n.Id == notificationId);
                if (notification != null)
                {
                    notification.IsRead = true;
                    break;
                }
            }
            
            return Task.CompletedTask;
        }

        public Task MarkAllAsReadAsync(string userId)
        {
            if (_userNotifications.TryGetValue(userId, out var notifications))
            {
                foreach (var notification in notifications)
                {
                    notification.IsRead = true;
                }
            }
            
            return Task.CompletedTask;
        }

        private void StoreNotification(Notification notification)
        {
            if (string.IsNullOrEmpty(notification.UserId))
                return;

            if (!_userNotifications.TryGetValue(notification.UserId, out var userNotifications))
            {
                userNotifications = new List<Notification>();
                _userNotifications[notification.UserId] = userNotifications;
            }
            
            userNotifications.Add(notification);
            
            // Keep only latest 50 notifications per user to avoid memory issues
            if (userNotifications.Count > 50)
            {
                userNotifications = userNotifications
                    .OrderByDescending(n => n.CreatedAt)
                    .Take(50)
                    .ToList();
                _userNotifications[notification.UserId] = userNotifications;
            }
        }
    }
}
