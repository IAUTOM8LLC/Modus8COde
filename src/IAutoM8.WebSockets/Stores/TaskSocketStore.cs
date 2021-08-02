using IAutoM8.Global.Enums;
using IAutoM8.WebSockets.Sockets;
using IAutoM8.WebSockets.Stores.Interfaces;
using Microsoft.AspNetCore.SignalR;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IAutoM8.WebSockets.Stores
{
    public class TaskSocketStore : ITaskSocketStore
    {
        private readonly IHubContext<TaskHub> _taskHub;
        public TaskSocketStore(IHubContext<TaskHub> taskHub)
        {
            _taskHub = taskHub;
        }

        public async Task TaskStatusChanged(int projectId, int taskId, TaskStatusType status)
        {
            await TaskStatusChanged(projectId, new Dictionary<int, TaskStatusType> { [taskId] = status });
        }

        public async Task TaskStatusChanged(int projectId, Dictionary<int, TaskStatusType> taskStatus)
        {
            await _taskHub.Clients.Group(projectId.ToString())
                .SendAsync("taskStatusChanged",
                    taskStatus.Select(pair => new ArrayList { pair.Key, pair.Value.ToString() }), projectId);
        }
    }
}
