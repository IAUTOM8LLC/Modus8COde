using IAutoM8.Neo4jRepository.Dto;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IAutoM8.Neo4jRepository.Interfaces
{
    public interface IFormulaTaskNeo4jRepository
    {
        ITransaction BeginTransaction();
        Task<List<TaskResourceNeo4jDto>> GetTaskResourcesAsync(int taskId);
        Task<List<TaskResourceNeo4jDto>> GetTaskAndSharedResourcesAsync(int taskId, int projectId);
        Task AddTasksWithResourcesAsync(
            IEnumerable<ImportTaskNeo4jDto> tasks,
            IEnumerable<TaskDependencyNeo4jDto> dependencies,
            IEnumerable<TaskConditionNeo4jDto> conditions);
        Task AddResourceToTaskAsync(int taskId, TaskResourceNeo4jDto resourceDto);

        Task AddTaskAsync(int taskId, int formulaId);
        Task DeleteTaskAsync(int taskId);
        Task<bool> HasLoopAsync(int taskId);

        #region dependency and condition

        Task AddTaskDependencyAsync(int parentTaskId, int childTaskId);
        Task RemoveTaskDependencyAsync(int parentTaskId, int childTaskId);
        Task AddTaskConditionAsync(int conditionId, int conditionTaskId, int taskId);
        Task RemoveTaskConditionAsync(int conditionTaskId, int taskId);

        #endregion
    }
}
