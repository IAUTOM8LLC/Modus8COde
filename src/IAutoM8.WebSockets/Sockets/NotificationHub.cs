using IAutoM8.WebSockets.Stores.Interfaces;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;

namespace IAutoM8.WebSockets.Sockets
{
    public class NotificationHub : Hub
    {
        private readonly INotificationSocketStore _store;

        public NotificationHub(INotificationSocketStore store)
        {
            _store = store;
        }

        public async Task SubscribeToUser(Guid user)
        {
            await Groups.AddAsync(Context.ConnectionId, user.ToString());
        }
    }
}
