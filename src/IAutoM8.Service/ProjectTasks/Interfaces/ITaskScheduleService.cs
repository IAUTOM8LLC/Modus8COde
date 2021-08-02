using IAutoM8.Domain.Models;
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
    public interface ITaskScheduleService
    {
        Task<RecurrenceAsapDto> GetNextOccurence(ITransactionScope trx, int taskId, RecurrenceOptions recurrenceOptions);
        Task ScheduleNewTask(ITransactionScope trx, ProjectTask task, bool isAsap = false);
        Task UpdateTaskTree(ITransactionScope trx, ProjectTask taskOld, ProjectTask task, UpdateTaskDto model, List<int> oldConditions);
        Task ChangeTaskStatus(
            ITransactionScope trx,
            ProjectTask task,
            TaskStatusType oldStatus,
            int? selectedConditionOptionId = null);
    }
}
