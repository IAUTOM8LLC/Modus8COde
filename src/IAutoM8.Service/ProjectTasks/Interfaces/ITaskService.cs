using IAutoM8.Domain.Models.Formula.Task;
using IAutoM8.Domain.Models.Project;
using IAutoM8.Domain.Models.Project.Task;
using IAutoM8.Global.Enums;
using IAutoM8.Repository;
using IAutoM8.Service.CommonDto;
using IAutoM8.Service.ProjectTasks.Dto;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IAutoM8.Service.ProjectTasks.Interfaces
{
    public interface ITaskService
    {
        Task<List<TaskDto>> GetTasksAsync(IEnumerable<int> projectIds, bool selectTodayTasks = false);
        Task<TaskDto> GetTaskAsync(int taskId);
        Task<TaskDto> AddTaskAsync(UpdateTaskDto model);
        Task<TaskDto> UpdateTaskAsync(int taskId, UpdateTaskDto model);
        Task<TaskDto> UpdateTaskStatusAsync(int taskId, StatusDto model);
        Task<bool> PublishTaskResourceAsync(int taskId, PublishTaskResourceDto model);
        Task<TaskDto> UpdateTaskNotificationAsync(int taskId, NotificationDto model);
        Task UpdateTasksPositionAsync(List<TaskPositionDto> list);
        Task DeleteTask(int taskId);
        Task AddTaskDependency(TaskDependencyDto model);
        Task RemoveTaskDependency(TaskDependencyDto model);
        Task DeleteTasks(ITransactionScope trx, IEnumerable<int> taskIds);
        Task DeleteProjectTasksAsync(ITransactionScope trx, int projectId);
        Task AssignTaskToConditionOption(int conditionOptionId, int? taskId);
        Task CompleteConditionalTask(int taskId, ConditionOptionDto model);
        Task<TaskDto> DoTask(int taskId);
        Task<TaskDto> ReviewTask(int taskId);
        Task<TaskDto> DoVendorTask(int taskId);
        Task<TaskDto> ChangeProcessingUser(ProcessingUserDto processingUserDto);
        Task<TaskDto> ChangeNewProcessingUser(ProcessingUserDto processingUserDto);
        Task<TaskDto> StopOutsource(int taskId);
        Task<ListViewTaskDto> GetTaskInStatusById(int taskId);
        Task<HomeDashboardDto> GetTasksInStatusAsync(Guid userId);
        Task<IList<ListViewTaskDto>> GetVendorTasksInStatusAsync(Guid userId);
        Task<IList<ListViewTaskDto>> GetTasksInStatusAsync(IEnumerable<int> projectIds,
            string statuses,
            int skip,
            int take,
            bool selectTasksForToday = false,
            Guid? userIdToLoad = null
        );

        Task<List<ProjectNotesDto>> GetTaskNotesAsync(int? taskId);
        Task<ProjectNotesDto> AddTaskNotesAsync(AddProjectNotesDto model);
        Task ShareProjectNote(AddProjectNotesDto model, List<int> sharedIds);
        Task<ProjectNotesDto> UpdateTaskNotesAsync(int noteId, bool isPublished);
        Task DeleteTaskNotesAsync(int noteId);
        Task<IList<VendorJobInvitesDto>> GetVendorJobInvites(Guid UserId);
        Task StopVendorTaskOnCancelNudge(int taskId);
        Task UpdateExpiredJobInvitesToOwner(int taskId);
        Task UpdateTaskChecklist(List<UpdateTaskChecklistDto> todos);
        Task<IList<int>> GetDownstreamShareIds(int taskId);
        Task<IList<int>> GetFormulaShareIds(int taskId);
        Task<List<TaskDto>> GetProjectTaskResourcesAsync(int projectId);
    }
}
