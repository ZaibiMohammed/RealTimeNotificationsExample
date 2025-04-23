using Microsoft.AspNetCore.SignalR;
using RealTimeNotifications.Core.Models;

namespace RealTimeNotifications.Infrastructure.Hubs
{
    public class NotificationHub : Hub
    {
        private static readonly Dictionary<string, string> _userConnections = new();

        public override async Task OnConnectedAsync()
        {
            string userId = Context.GetHttpContext()?.Request.Query["userId"].ToString() ?? string.Empty;
            
            if (!string.IsNullOrEmpty(userId))
            {
                _userConnections[userId] = Context.ConnectionId;
                await Groups.AddToGroupAsync(Context.ConnectionId, userId);
            }
            
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            string userId = _userConnections.FirstOrDefault(x => x.Value == Context.ConnectionId).Key;
            
            if (!string.IsNullOrEmpty(userId))
            {
                _userConnections.Remove(userId);
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, userId);
            }
            
            await base.OnDisconnectedAsync(exception);
        }

        public async Task JoinGroup(string groupName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        }

        public async Task LeaveGroup(string groupName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        }
    }
}
