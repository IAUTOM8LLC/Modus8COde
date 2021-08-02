using IAutoM8.WebSockets.Sockets;
using IAutoM8.WebSockets.Stores;
using IAutoM8.WebSockets.Stores.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace IAutoM8.WebSockets.Infrastructure
{
    public static class Module
    {
        public static void Configure(IApplicationBuilder app)
        {
            app.UseSignalR(routes =>
            {
                routes.MapHub<TaskHub>("/task");
                routes.MapHub<NotificationHub>("/notification-hub");
            });
            app.UseSignalR(routes =>
            {
                routes.MapHub<LoginHub>("/login-hub");
            });
        }

        public static void ConfigureServices(IServiceCollection services)
        {
            services.AddSignalR();

            services.AddScoped<ITaskSocketStore, TaskSocketStore>();
            services.AddScoped<ILoginSocketStore, LoginSocketStore>();
            services.AddScoped<INotificationSocketStore, NotificationSocketStore>();
        }
    }
}
