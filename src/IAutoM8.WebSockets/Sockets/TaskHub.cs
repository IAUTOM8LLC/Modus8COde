using IAutoM8.WebSockets.Stores.Interfaces;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace IAutoM8.WebSockets.Sockets
{
    public class TaskHub : Hub
    {
        private readonly ITaskSocketStore _store;

        public TaskHub(ITaskSocketStore store)
        {
            _store = store;
        }

        public async Task SubscribeToProject(int projectId)
        {
            await Groups.AddAsync(Context.ConnectionId, projectId.ToString());
        }

        public async Task SubscribeToProjects(int[] projectIds)
        {
            foreach (var projectId in projectIds)
            {
                await Groups.AddAsync(Context.ConnectionId, projectId.ToString());
            }
        }
    }
}
