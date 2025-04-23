using Microsoft.Extensions.DependencyInjection;
using RealTimeNotifications.Core.Interfaces;
using RealTimeNotifications.Infrastructure.Hubs;
using RealTimeNotifications.Infrastructure.Services;

namespace RealTimeNotifications.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
        {
            services.AddSignalR();
            services.AddSingleton<INotificationService, NotificationService>();
            
            return services;
        }
    }
}
