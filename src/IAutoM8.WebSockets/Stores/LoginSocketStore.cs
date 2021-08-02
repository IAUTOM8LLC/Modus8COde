using IAutoM8.WebSockets.Sockets;
using IAutoM8.WebSockets.Stores.Interfaces;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;

namespace IAutoM8.WebSockets.Stores
{
    public class LoginSocketStore : ILoginSocketStore
    {
        private readonly IHubContext<LoginHub> _loginHub;
        public LoginSocketStore(IHubContext<LoginHub> loginHub)
        {
            _loginHub = loginHub;
        }

        public async Task LogOff(Guid id)
        {
            await _loginHub.Clients.Group(id.ToString())
                .SendAsync("logOff");
        }
    }
}
