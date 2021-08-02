using IAutoM8.Global.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IAutoM8.WebSockets.Stores.Interfaces
{
    public interface ITaskSocketStore
    {
        Task TaskStatusChanged(int projectId, int taskId, TaskStatusType status);
        Task TaskStatusChanged(int projectId, Dictionary<int, TaskStatusType> taskStatus);
    }
}
