using IAutoM8.Global.Enums;
using IAutoM8.Neo4jRepository.Dto;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IAutoM8.Neo4jRepository.Interfaces
{
    public interface ITaskNeo4jRepository
    {
        ITransaction BeginTransaction();
        #region resources
        Task<List<TaskResourceNeo4jDto>> GetTaskAndSharedResourcesAsync(int taskId, int projectId);
        /// <summary>
        /// Get all task resource of project
        /// </summary>
        /// <param name="taskId"></param>
        /// <param name="projectId"></param>
        /// <returns></returns>
        Task<List<TaskResourceNeo4jDto>> GetTaskAndAllSharedResourcesAsync(int taskId, int projectId);
        Task<List<TaskResourceNeo4jDto>> GetTaskResourcesAsync(int taskId);
        /// <summary>
        /// Get all task resource of project for owner and managers
        /// </summary>
        Task<IEnumerable<TaskResourceInfoNeo4jDto>> GetProjectResourcesAsync(int projectId);
        /// <summary>
        /// Get all task resource of project for workers
        /// </summary>
        Task<IEnumerable<TaskResourceInfoNeo4jDto>> GetProjectResourcesAsync(int projectId, IEnumerable<int> taskIds);
        #endregion
        Task AddTasksWithResourcesAsync(
            IEnumerable<ImportTaskNeo4jDto> tasks,
            IEnumerable<TaskDependencyNeo4jDto> dependencies,
            IEnumerable<TaskConditionNeo4jDto> conditions);
        Task AddResourceToTaskAsync(int taskId, TaskResourceNeo4jDto resourceDto);
        Task AddResourceToFormulaTaskAsync(int taskId, TaskResourceNeo4jDto resourceDto);

        Task AddTaskAsync(int taskId, int projectId);
        Task AddFormulaTaskAsync(int taskId, int projectId, int formulaTaskId);
        Task ChangeTaskStatusAsync(int taskId, TaskStatusType type);
        Task DeleteTaskAsync(int taskId);
        Task DeleteAllProjectTasksAsync(int projectId);
        Task<IEnumerable<int>> GetAllTaskWithResourcesAsync(int projectId);
        Task<bool> HasLoopAsync(int taskId);

        #region dependency and condition
        Task AddTaskDependencyAsync(int parentTaskId, int childTaskId);
        Task RemoveTaskDependencyAsync(int parentTaskId, int childTaskId);
        Task AddTaskConditionAsync(int conditionId, int conditionTaskId, int taskId);
        Task RemoveTaskConditionAsync(int conditionTaskId, int taskId);
        Task SetTaskConditionSelectedAsync(int conditionId, bool isSelected);
        Task SetTaskConditionSelectedAsync(int taskCondId, int taskChildId);
        Task DeselectTaskConditionsAsync(int taskId);
        #endregion

        Task<bool> IsLeafAsync(int taskId);
        Task<bool> IsRootAsync(int taskId);
        Task<bool> HasRelationsAsync(int taskId1, int taskId2);
        Task<IEnumerable<int>> GetChildTaskIdsAsync(int taskId);
        Task<IEnumerable<int>> GetParentTaskIdsAsync(int taskId);
        Task<IEnumerable<int>> GetRootTaskIdsAsync(int taskId);
        Task<IEnumerable<int>> GetProjectRootTaskIdsAsync(int projectId);
        Task<IEnumerable<int>> GetReadyToStartTaskIdsAsync(int taskId);
        Task<IEnumerable<int>> GetFormulaRootTaskIdsAsync(int formulaTaskId);
        Task<IEnumerable<int>> GetFormulaRootAllTaskIdsAsync(int formulaTaskId);
        Task<bool> IsGraphCompleted(int taskId);
        Task<bool> IsFormulaGraphCompletedAsync(int forumlaTaskId);
        Task<bool> IsFormulaRootCompletedAsync(int forumlaTaskId);
        Task<bool> IsAvailableToStartAsync(int taskId);
        Task PublishTaskResourceAsync(PublishTaskResourceNeo4jDto resourceDto);
    }
}
