using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IAutoM8.Domain.Models.Project.Task;
using IAutoM8.Global.Enums;
using IAutoM8.Neo4jRepository.Interfaces;
using IAutoM8.Repository;
using IAutoM8.Service.CommonService.Interfaces;
using IAutoM8.Service.ProjectTasks.Interfaces;
using IAutoM8.Service.Scheduler.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace IAutoM8.Service.ProjectTasks
{
    public class FormulaTaskJobService : IFormulaTaskJobService
    {
        private readonly IScheduleService _scheduleService;
        private readonly ITaskNeo4jRepository _taskNeo4JRepository;
        private readonly ITaskScheduleService _taskScheduleService;
        private readonly ITaskStartDateHelperService _startDateHelperService;

        public FormulaTaskJobService(IScheduleService scheduleService,
            ITaskNeo4jRepository taskNeo4JRepository,
            ITaskScheduleService taskScheduleService,
            ITaskStartDateHelperService startDateHelperService)
        {
            _scheduleService = scheduleService;
            _taskNeo4JRepository = taskNeo4JRepository;
            _taskScheduleService = taskScheduleService;
            _startDateHelperService = startDateHelperService;
        }

        public async Task RemoveFormulaTaskJobs(ITransactionScope trx, ProjectTask childTask, bool isFormulaTaskRoot)
        {
            if (childTask.Status == TaskStatusType.New)
            {
                await _scheduleService.RemoveJob(trx, childTask.Id);
                if (isFormulaTaskRoot)
                {
                    var rootTaskIds = await _taskNeo4JRepository.GetFormulaRootTaskIdsAsync(childTask.Id);
                    foreach (var task in await trx.Read<ProjectTask>()
                        .Where(w => w.FormulaId.HasValue && w.Status == TaskStatusType.New && rootTaskIds.Contains(w.Id)).ToListAsync())
                    {
                        await RemoveFormulaTaskJobs(trx, task, true);
                    }
                    foreach (var formulaTaskId in rootTaskIds)
                    {
                        await _scheduleService.RemoveJob(trx, formulaTaskId);
                    }
                }
            }
        }

        public async Task AddFormulaTaskJobs(ITransactionScope trx, ProjectTask task)
        {
            var formulaRootIds = await _taskNeo4JRepository.GetFormulaRootTaskIdsAsync(task.Id);
            var formulaTasks = await trx.Track<ProjectTask>()
                .Include(i => i.RecurrenceOptions)
                .Where(w => formulaRootIds.Contains(w.Id))
                .ToListAsync();
            foreach (var formulaTask in formulaTasks)
            {
                if (formulaTask.FormulaId.HasValue)
                {
                    await AddFormulaTaskJobs(trx, formulaTask);
                }
                else
                {
                    await _taskScheduleService.ScheduleNewTask(trx, formulaTask);
                }
            }
        }

        public async Task UpdateFormulaTaskTime(ITransactionScope trx, ProjectTask task)
        {
            if (task.FormulaId.HasValue)
            {
                var parentIds = await _taskNeo4JRepository.GetParentTaskIdsAsync(task.Id);
                var date = await trx.Read<ProjectTask>().Where(w => parentIds.Contains(w.Id)).Select(s => s.RecurrenceOptions == null ?
                       s.StartDate.Value.AddMinutes(s.Duration.Value) : s.RecurrenceOptions.NextOccurenceDate.Value.AddMinutes(s.Duration.Value))
                    .OrderByDescending(o => o).FirstOrDefaultAsync();
                var result = await _startDateHelperService.InitTasksStartDate(trx, task.ProjectId, new Formula.Dto.ProjectStartDatesDto
                {
                    ProjectStartDateTime = date
                },
                await _taskNeo4JRepository.GetFormulaRootTaskIdsAsync(task.Id));
                task.StartDate = result.StartTime;
                task.Duration = result.TotalDuration;
                await trx.SaveChangesAsync();
            }
        }

        public async Task ScheduleTaskJobs(ProjectTask task, ITransactionScope trx)
        {
            if (task.Status == TaskStatusType.New &&
                await _taskNeo4JRepository.IsRootAsync(task.Id))
            {
                if (task.FormulaId.HasValue)
                {
                    await AddFormulaTaskJobs(trx, task);
                }
                else
                {
                    if (task.RecurrenceOptionsId.HasValue)
                    {
                        var recurrenceDetail = await _taskScheduleService.GetNextOccurence(trx, task.Id, task.RecurrenceOptions);
                        task.RecurrenceOptions.NextOccurenceDate = recurrenceDetail.StartFrom;
                        task.RecurrenceOptions.Cron = recurrenceDetail.Cron;
                    }

                    await _taskScheduleService.ScheduleNewTask(trx, task, task.RecurrenceOptions?.IsAsap ?? false);
                }
            }
        }

        public async Task TryResetProjectTaskTreeStatuses(TaskStatusType status, int parentTaskId, int childTaskId)
        {
            if (status == TaskStatusType.Completed
                && !await _taskNeo4JRepository.HasRelationsAsync(parentTaskId, childTaskId)
                && await _taskNeo4JRepository.IsGraphCompleted(parentTaskId))
            {
                await _scheduleService.ResetProjectTaskTreeStatuses(parentTaskId);
            }
        }
    }
}
