using IAutoM8.WebSockets.Stores.Interfaces;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;

namespace IAutoM8.WebSockets.Sockets
{
    public class LoginHub : Hub
    {
        private readonly ILoginSocketStore _store;

        public LoginHub(ILoginSocketStore store)
        {
            _store = store;
        }

        public async Task SubscribeLogin(Guid id)
        {
            await Groups.AddAsync(Context.ConnectionId, id.ToString());
        }
    }
}
