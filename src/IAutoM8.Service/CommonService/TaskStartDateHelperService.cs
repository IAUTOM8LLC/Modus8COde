using IAutoM8.Domain.Models.Project.Task;
using IAutoM8.Neo4jRepository.Interfaces;
using IAutoM8.Repository;
using IAutoM8.Service.CommonDto;
using IAutoM8.Service.CommonService.Interfaces;
using IAutoM8.Service.Formula.Dto;
using IAutoM8.Service.ProjectTasks.Interfaces;
using Microsoft.EntityFrameworkCore;
using NCrontab.Advanced.Enumerations;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace IAutoM8.Service.CommonService
{
    public class TaskStartDateHelperService : ITaskStartDateHelperService
    {
        private readonly IDateTimeService _dateTimeService;
        private readonly ITaskScheduleService _taskScheduleService;
        private readonly ITaskNeo4jRepository _taskNeo4JRepository;

        public TaskStartDateHelperService(IDateTimeService dateTimeService,
                                          ITaskScheduleService taskScheduleService,
                                          ITaskNeo4jRepository taskNeo4JRepository)
        {
            _dateTimeService = dateTimeService;
            _taskScheduleService = taskScheduleService;
            _taskNeo4JRepository = taskNeo4JRepository;
        }

        public async Task<InitStartDateResultDto> InitTasksStartDate(ITransactionScope trx,
                                                                     int projectId,
                                                                     ProjectStartDatesDto startDates,
                                                                     IEnumerable<int> taskIds)
        {
            // Excluded the isDisabled = true from the root tasks from getting started
            var rootTasks = await trx.Track<ProjectTask>()
                .Include(t => t.RecurrenceOptions)
                .Where(w => w.ProjectId == projectId && taskIds.Contains(w.Id))
                .Where(w => !w.IsDisabled.HasValue || (w.IsDisabled.HasValue && !w.IsDisabled.Value))
                .ToListAsync();

            // Code added if in case the first task in the container is disabled
            if (rootTasks.Count == 0)
            {
                var rootProjectId = await trx.Track<ProjectTask>()
                    .Where(w => w.ProjectId == projectId && w.FormulaId.HasValue)
                    .Select(t => t.Id)
                    .FirstOrDefaultAsync();

                var treeIds = (await _taskNeo4JRepository.GetFormulaRootAllTaskIdsAsync(rootProjectId)).ToList();
                var filteredId = treeIds[1];

                rootTasks = await trx.Track<ProjectTask>()
                    .Include(t => t.RecurrenceOptions)
                    .Where(w => w.Id == filteredId)
                    .ToListAsync();
            }

            var result = new InitStartDateResultDto { RootTasks = rootTasks };
            var endTimes = new List<DateTime>();
            foreach (var rootTask in rootTasks)
            {
                result.RootTasks = result.RootTasks
                    .Concat(await SetStartDatesForChain(trx, rootTask, startDates, endTimes));
            }

            var startTime = rootTasks
                .Select(s => s.RecurrenceOptions?.NextOccurenceDate ?? s.StartDate.Value)
                .OrderBy(o => o)
                .FirstOrDefault();
            result.RootTasks = result.RootTasks.Where(w => !w.FormulaId.HasValue);
            var endTime = endTimes.OrderByDescending(o => o).FirstOrDefault();
            result.StartTime = startTime;
            result.TotalDuration = (int)(endTime - startTime).TotalMinutes;
            return result;
        }

        private async Task<IEnumerable<ProjectTask>> SetStartDatesForChain(ITransactionScope trx, ProjectTask rootTask,
            ProjectStartDatesDto startDates, List<DateTime> endTimes)
        {
            IEnumerable<ProjectTask> rootTasks;
            DateTime endDateTime;
            if (rootTask.FormulaId.HasValue)
            {
                var rootIds = await _taskNeo4JRepository.GetFormulaRootTaskIdsAsync(rootTask.Id);
                var result = await InitTasksStartDate(trx, rootTask.ProjectId, startDates, rootIds);
                rootTask.StartDate = result.StartTime;
                rootTask.Duration = result.TotalDuration;
                rootTasks = result.RootTasks;
                endDateTime = result.StartTime.AddMinutes(result.TotalDuration);
            }
            else
            {
                var startDateTime = startDates.ProjectStartDateTime ?? _dateTimeService.NowUtc;

                if ((startDates.RootStartDateTime?.ContainsKey(rootTask.Id) ?? false)
                    && startDates.RootStartDateTime[rootTask.Id].HasValue)
                {
                    startDateTime = startDates.RootStartDateTime[rootTask.Id].Value;
                }

                endDateTime = SetStartDate(rootTask, startDateTime);
                rootTasks = new List<ProjectTask>();
            }
            endTimes.Add(endDateTime);

            var childIds = await _taskNeo4JRepository.GetChildTaskIdsAsync(rootTask.Id);
            if (childIds.Any())
            {
                var childTasks = await trx.Track<ProjectTask>()
                    .Include(t => t.RecurrenceOptions)
                    .Where(w => w.ProjectId == rootTask.ProjectId && childIds.Contains(w.Id))
                    .Where(w => !w.IsDisabled.HasValue || (w.IsDisabled.HasValue && !w.IsDisabled.Value))
                    .ToListAsync();

                // w.isDiabled.HasValue
                await HandleChildTasks(trx, childTasks, endDateTime, endTimes);
            }
            return rootTasks;
        }

        private DateTime SetStartDate(ProjectTask task, DateTime parentEnd)
        {
            if (task.RecurrenceOptionsId.HasValue)
            {
                var recurrenceDetails = _dateTimeService.ParseRecurrenceAsap(task.RecurrenceOptions, parentEnd);
                task.RecurrenceOptions.Cron = recurrenceDetails.Cron;
                var newStartDate = _dateTimeService.GetNextOccurence(recurrenceDetails);

                task.StartDate = task.StartDate.HasValue && task.StartDate > newStartDate ? task.StartDate : newStartDate;
                task.RecurrenceOptions.NextOccurenceDate = task.StartDate;
            }
            else
            {
                var newStartDate = parentEnd.AddMinutes(task.StartDelay);
                task.StartDate = task.StartDate.HasValue && task.StartDate > newStartDate ? task.StartDate : newStartDate;
            }

            return task.StartDate.Value.AddMinutes(task.Duration ?? 0);
        }

        private async Task HandleChildTasks(ITransactionScope trx,
                                            List<ProjectTask> tasks,
                                            DateTime parentEndDate,
                                            List<DateTime> endTimes)
        {
            foreach (var task in tasks)
            {
                DateTime endDateTime;
                if (task.FormulaId.HasValue)
                {
                    var rootIds = await _taskNeo4JRepository.GetFormulaRootTaskIdsAsync(task.Id);
                    var result = await InitTasksStartDate(trx, task.ProjectId, new ProjectStartDatesDto
                    {
                        ProjectStartDateTime = parentEndDate
                    }, rootIds);
                    task.StartDate = result.StartTime;
                    task.Duration = result.TotalDuration;
                    endDateTime = result.StartTime.AddMinutes(result.TotalDuration);
                }
                else
                {
                    endDateTime = SetStartDate(task, parentEndDate);
                }
                endTimes.Add(endDateTime);
                var childIds = await _taskNeo4JRepository.GetChildTaskIdsAsync(task.Id);

                if (childIds.Any())
                {
                    var childTasks = await trx.Track<ProjectTask>()
                        .Include(t => t.RecurrenceOptions)
                        .Where(w => w.ProjectId == task.ProjectId && childIds.Contains(w.Id))
                        .Where(w => !w.IsDisabled.HasValue || (w.IsDisabled.HasValue && !w.IsDisabled.Value))
                        .ToListAsync();

                    await HandleChildTasks(trx, childTasks, endDateTime, endTimes);
                }
            }
        }

        public async Task UpdateStartDatesForTreeIfNeeded(ITransactionScope trx, ProjectTask parentTask, ProjectTask childTask)
        {
            var childStartDate = childTask.RecurrenceOptions?.NextOccurenceDate ?? childTask.StartDate;
            var parentStartDate = parentTask.RecurrenceOptions?.NextOccurenceDate ?? parentTask.StartDate;

            Debug.Assert(childStartDate != null, "childStartDate != null");
            Debug.Assert(parentStartDate != null, "parentStartDate != null");
            Debug.Assert(parentTask.Duration != null, "parentTask.Duration != null");

            if (parentTask.IsInterval && !childTask.IsInterval
                && !parentTask.ParentTasks.Any() && !parentTask.AssignedConditionOptions.Any())
            {
                parentTask.StartDate =
                    _dateTimeService.MaxDate(childStartDate.Value.AddMinutes(-parentTask.Duration.Value),
                        _dateTimeService.NowUtc);
                await trx.SaveChangesAsync();
            }

            if (childStartDate < parentStartDate.Value.AddMinutes(parentTask.Duration.Value) || childTask.IsInterval)
            {
                var tasks = await trx.Track<ProjectTask>()
                    .Include(i => i.ChildTasks)
                    .Include(i => i.ParentTasks)
                    .Include(i => i.RecurrenceOptions)
                    .Include(i => i.Condition)
                        .ThenInclude(i => i.Options)
                    .Where(w => w.ProjectId == childTask.ProjectId)
                    .ToListAsync();

                var startAfter = parentStartDate.Value.AddMinutes(parentTask.Duration ?? 1);
                await UpdateStartDate(trx, tasks, childTask.Id, startAfter);
                await trx.SaveChangesAsync();
            }
        }

        private async Task UpdateStartDate(ITransactionScope trx, List<ProjectTask> tasks, int taskId, DateTime startAfter)
        {
            var task = tasks.First(t => t.Id == taskId);
            var estimatedStartAfter = (DateTime?)null;
            if (task.IsInterval)
            {
                task.StartDate = startAfter.AddMinutes(1);
                estimatedStartAfter = task.StartDate.Value.AddMinutes(task.Duration ?? 1);
            }
            else if (task.RecurrenceOptions != null && task.RecurrenceOptions.NextOccurenceDate < startAfter)
            {
                var recurrenceDetail = await _taskScheduleService.GetNextOccurence(trx, task.Id, task.RecurrenceOptions);
                task.RecurrenceOptions.NextOccurenceDate = recurrenceDetail.StartFrom;
                task.RecurrenceOptions.Cron = recurrenceDetail.Cron;

                // ReSharper disable once PossibleInvalidOperationException
                estimatedStartAfter = task.RecurrenceOptions.NextOccurenceDate.Value.AddMinutes(task.Duration.Value);
            }
            else if (task.StartDate < startAfter)
            {
                task.StartDate = startAfter.AddMinutes(5);//Magical Number
                estimatedStartAfter = task.StartDate.Value.AddMinutes(task.Duration ?? 1);

            }
            if (estimatedStartAfter == null) return;

            if (task.ChildTasks.Any())
            {
                foreach (var childTask in task.ChildTasks)
                {
                    await UpdateStartDate(trx, tasks, childTask.ChildTaskId, estimatedStartAfter.Value);
                }
            }

            if (task.Condition != null && task.Condition.Options.Any(o => o.AssignedTaskId.HasValue))
            {
                foreach (var option in task.Condition.Options.Where(o => o.AssignedTaskId.HasValue))
                {
                    // ReSharper disable once PossibleInvalidOperationException
                    await UpdateStartDate(trx, tasks, option.AssignedTaskId.Value, estimatedStartAfter.Value);
                }
            }
        }
    }
}
