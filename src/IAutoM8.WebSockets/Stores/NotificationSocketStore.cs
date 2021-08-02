using IAutoM8.WebSockets.Sockets;
using IAutoM8.WebSockets.Stores.Interfaces;
using IAutoM8.WebSockets.Stores.Models;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IAutoM8.WebSockets.Stores
{
    public class NotificationSocketStore : INotificationSocketStore
    {
        private readonly IHubContext<NotificationHub> _hub;
        public NotificationSocketStore(IHubContext<NotificationHub> hub)
        {
            _hub = hub;
        }

        public async Task TaskNewNotification(Guid user, NotificationModel message)
        {
            await _hub.Clients.Group(user.ToString())
                .SendAsync("newMessageRecieved", message);
        }

        public async Task TaskNewNotification(IEnumerable<Guid> users, NotificationModel message)
        {
            throw new Exception();
        }

        public async Task TaskUnreadNotificationCount(Guid user, int count)
        {
            await _hub.Clients.Group(user.ToString())
                   .SendAsync("unreadMessageCount",
                       count);
        }
    }
}
