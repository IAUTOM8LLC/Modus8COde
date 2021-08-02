using IAutoM8.Global.Enums;
using IAutoM8.Repository;
using IAutoM8.Service.ProjectTasks.Dto;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IAutoM8.Service.ProjectTasks.Interfaces
{
    public interface ITaskHistoryService
    {
        Task<List<TaskHistoryItemDto>> GetTaskHistoryByProjectId(int projectId, int count = 50);
        Task<List<TaskHistoryItemDto>> GetTasksHistory(IEnumerable<int> projectIds, int count = 50);

        Task Write(
            int taskId,
            ActivityType activityType,
            int? selectedConditionOptionId = null,
            ITransactionScope trx = null,
            bool saveChanges = false);
    }
}
