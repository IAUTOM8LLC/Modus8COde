using IAutoM8.Domain.Models;
using IAutoM8.Domain.Models.Credits;
using IAutoM8.Domain.Models.Project.Task;
using IAutoM8.Domain.Models.Skill;
using IAutoM8.Domain.Models.User;
using IAutoM8.Domain.Models.Vendor;
using IAutoM8.Global;
using IAutoM8.Global.Enums;
using IAutoM8.Global.Exceptions;
using IAutoM8.Global.Options;
using IAutoM8.Neo4jRepository.Interfaces;
using IAutoM8.Repository;
using IAutoM8.Service.Braintree.Interfaces;
using IAutoM8.Service.CommonDto;
using IAutoM8.Service.CommonService.Interfaces;
using IAutoM8.Service.Infrastructure.Extensions;
using IAutoM8.Service.Notification.Interfaces;
using IAutoM8.Service.ProjectTasks.Dto;
using IAutoM8.Service.ProjectTasks.Interfaces;
using IAutoM8.Service.Scheduler.Interfaces;
using IAutoM8.Service.SendGrid.Interfaces;
using IAutoM8.WebSockets.Stores.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace IAutoM8.Service.ProjectTasks
{
    public class TaskScheduleService : ITaskScheduleService
    {
        private readonly IScheduleService _scheduleService;
        private readonly ClaimsPrincipal _principal;
        private readonly IDateTimeService _dateTimeService;
        private readonly INotificationService _notificationService;
        private readonly ITaskNeo4jRepository _taskNeo4JRepository;
        private readonly ITaskHistoryService _taskHistoryService;
        private readonly ITaskSocketStore _taskSocketStore;
        private readonly Func<ITaskStartDateHelperService> _startDateHelperServiceFactory;
        private readonly UserManager<User> _userManager;

        public TaskScheduleService(
            IScheduleService scheduleService,
            ClaimsPrincipal principal,
            IDateTimeService dateTimeService,
            INotificationService notificationService,
            ITaskNeo4jRepository taskNeo4JRepository,
            ITaskHistoryService taskHistoryService,
            ITaskSocketStore taskSocketStore,
            UserManager<User> userManager,
            Func<ITaskStartDateHelperService> startDateHelperServiceFactory)
        {
            _scheduleService = scheduleService;
            _principal = principal;
            _dateTimeService = dateTimeService;
            _notificationService = notificationService;
            _taskNeo4JRepository = taskNeo4JRepository;
            _taskHistoryService = taskHistoryService;
            _taskSocketStore = taskSocketStore;
            _startDateHelperServiceFactory = startDateHelperServiceFactory;
            _userManager = userManager;
        }

        public async Task ScheduleNewTask(ITransactionScope trx, ProjectTask task, bool isAsap = false)
        {
            if (task.IsAutomated)
            {
                await _scheduleService.CreateAutomatedJob(trx, task, isAsap);
            }
            else
            {
                await _scheduleService.CreateJobBegin(trx, task, isAsap);
            }
        }

        public async Task<RecurrenceAsapDto> GetNextOccurence(ITransactionScope trx, int taskId, RecurrenceOptions recurrenceOptions)
        {
            var lastParentTaskTime = await trx.Read<ProjectTaskDependency>()
                .Include(i => i.ParentTask)
                .ThenInclude(i => i.RecurrenceOptions)
                .Where(x => x.ChildTaskId == taskId)
                .Select(s => s.ParentTask.RecurrenceOptionsId.HasValue
                    ? s.ParentTask.RecurrenceOptions.NextOccurenceDate.Value.AddMinutes(s.ParentTask.Duration.Value)
                    : s.ParentTask.StartDate.Value.AddMinutes(s.ParentTask.Duration.Value))
                .OrderByDescending(o => o)
                .FirstOrDefaultAsync();

            var lastAssignedTaskTime = await trx.Read<ProjectTaskConditionOption>()
                .Include(i => i.Condition)
                .ThenInclude(i => i.Task)
                .ThenInclude(i => i.RecurrenceOptions)
                .Where(x => x.AssignedTaskId == taskId)
                .Select(s => s.Condition.Task.RecurrenceOptionsId.HasValue
                    ? s.Condition.Task.RecurrenceOptions.NextOccurenceDate.Value.AddMinutes(s.Condition.Task.Duration.Value)
                    : s.Condition.Task.StartDate.Value.AddMinutes(s.Condition.Task.Duration.Value))
                .OrderByDescending(o => o)
                .FirstOrDefaultAsync();

            return _dateTimeService.ParseRecurrenceAsap(recurrenceOptions,
                _dateTimeService.MaxDate(
                    lastParentTaskTime,
                    lastAssignedTaskTime,
                    _dateTimeService.NowUtc));
        }

        public async Task ChangeTaskStatus(
            ITransactionScope trx,
            ProjectTask task,
            TaskStatusType newStatus,
            int? selectedConditionOptionId = null)
        {
            var oldStatus = task.Status;
            task.Status = newStatus;
            await trx.SaveChangesAsync();

            var userId = _principal.GetUserId();

            switch (oldStatus)
            {
                case TaskStatusType.InProgress:
                    if (!task.ProccessingUserGuid.HasValue)
                        throw new ValidationException("Nobody has said they've started this task yet.");

                    // Non Global Tasks
                    if (task.FormulaTask != null && !task.FormulaTask.FormulaProject.IsGlobal)
                    {
                        if (task.DescNotificationFlag.HasValue && task.DescNotificationFlag.Value)
                            throw new ValidationException("Description under Training tab is unread.");
                    }
                    else if (task.FormulaTask != null && task.FormulaTask.FormulaProject.IsGlobal && _principal.IsVendor())
                    {
                        // Global Tasks Vendor Tasks
                        if (task.DescNotificationFlag.HasValue && task.DescNotificationFlag.Value)
                            throw new ValidationException("Description under Training tab is unread.");
                    }

                    if (_principal.IsWorker())
                    {
                        if (task.ProccessingUserGuid.Value != userId)
                            throw new ValidationException("This task is being processed by another worker.");

                        switch (newStatus)
                        {
                            case TaskStatusType.NeedsReview:
                                if (!task.ProjectTaskUsers.Any(t => t.ProjectTaskUserType == ProjectTaskUserType.Reviewing))
                                    throw new ValidationException("Task have no assigned reviewing skill.");
                                task.Status = newStatus;
                                await _taskNeo4JRepository.ChangeTaskStatusAsync(task.Id, newStatus);
                                await _taskHistoryService.Write(task.Id, ActivityType.NeedsReview, selectedConditionOptionId, trx);
                                await _scheduleService.RemoveJob(trx, task.Id);
                                await _notificationService.SendNeedReviewTaskAsync(trx, task.Id);
                                break;

                            case TaskStatusType.Completed:
                                if (task.ProjectTaskUsers.Any(t => t.ProjectTaskUserType == ProjectTaskUserType.Reviewing))
                                    throw new ValidationException("Task have assigned reviewing skill.");

                                await CompleteTask(trx, task,
                                    selectedConditionOptionId);
                                break;

                            default:
                                throw new ValidationException("Cannot change to upcomming status manually.");
                        }
                    }
                    else if (_principal.IsVendor())
                    {
                        if (task.ProccessingUserGuid.Value != userId)
                            throw new ValidationException("This task is being processed by another worker.");

                        switch (newStatus)
                        {
                            case TaskStatusType.NeedsReview:
                                if (!task.ProjectTaskUsers.Any(t => t.ProjectTaskUserType == ProjectTaskUserType.Reviewing))
                                    throw new ValidationException("Task have no assigned reviewing skill.");
                                task.Status = newStatus;
                                await _taskNeo4JRepository.ChangeTaskStatusAsync(task.Id, newStatus);
                                await _taskHistoryService.Write(task.Id, ActivityType.NeedsReview, selectedConditionOptionId, trx);
                                await _scheduleService.RemoveJob(trx, task.Id);
                                await _notificationService.SendNeedReviewTaskAsync(trx, task.Id);
                                var statistic = await trx.Track<FormulaTaskStatistic>()
                                    .Where(w => w.VendorGuid == userId && w.ProjectTaskId == task.Id &&
                                        w.FormulaTaskId == task.FormulaTaskId && w.Type == StatisticType.Working)
                                    .OrderByDescending(o => o.Id).FirstAsync();
                                statistic.Completed = _dateTimeService.NowUtc;
                                statistic.Value = (short)(statistic.Completed.Value - statistic.Created).TotalMinutes;
                                if (statistic.FormulaTaskStatisticId.HasValue) {
                                    var oldStatistic = await trx.Track<FormulaTaskStatistic>()
                                        .FirstAsync(w => w.Id == statistic.FormulaTaskStatisticId);
                                    oldStatistic.Value += statistic.Value;
                                }
                                await _scheduleService.CreateVendorTaskReviewJob(trx, task.Id);
                                break;

                            default:
                                throw new ValidationException("Cannot change to upcomming status manually.");
                        }
                    }
                    else
                    {
                        var userHasSkill = task.ProjectTaskUsers
                            .Any(t => t.ProjectTaskUserType == ProjectTaskUserType.Assigned
                                      && t.UserId == userId);

                        //var isVendorTax = task.ProccessingUser.Roles.Any(r => r.Role.Name == UserRoles.Vendor || r.Role.Name == UserRoles.CompanyWorker); //added logic for new role CompanyWorker WRT Sprint 10B

                        var isVendorTax = task.ProccessingUser.Roles.Any(r => r.Role.Name == UserRoles.Vendor || r.Role.Name == UserRoles.CompanyWorker || r.Role.Name == UserRoles.Company); //added logic for new role Company WRT Sprint 10B

                        if (!userHasSkill || isVendorTax)
                            throw new ForbiddenException("You have no access to a task.");

                        switch (newStatus)
                        {
                            case TaskStatusType.NeedsReview:
                                if (!task.ProjectTaskUsers.Any(t => t.ProjectTaskUserType == ProjectTaskUserType.Reviewing))
                                    throw new ValidationException("Task have no assigned reviewing skill.");
                                task.Status = newStatus;
                                await _taskNeo4JRepository.ChangeTaskStatusAsync(task.Id, newStatus);
                                await _taskHistoryService.Write(task.Id, ActivityType.NeedsReview, selectedConditionOptionId, trx);
                                await _notificationService.SendNeedReviewTaskAsync(trx, task.Id);
                                break;

                            case TaskStatusType.Completed:
                                var reviewers = task.ProjectTaskUsers.Where(t =>
                                         t.ProjectTaskUserType == ProjectTaskUserType.Reviewing).Select(s => s.UserId).ToList();

                                if (reviewers.Count > 0 && reviewers.All(uId => uId != userId))
                                    throw new ForbiddenException("You don't have rigths to review this task.");

                                await CompleteTask(trx, task, selectedConditionOptionId);
                                break;

                            default:
                                throw new ValidationException("Cannot change to upcomming status manually.");
                        }
                    }

                    // Check if all todos are checked
                    var todo = await trx.Read<ProjectTaskChecklist>()
                        .Where(t => t.ProjectTaskId == task.Id && t.Type == TodoType.Resource && !t.TodoIsChecked)
                        .FirstOrDefaultAsync();

                    if (todo != null)
                        throw new ValidationException("Checklist items are pending.");

                    break;

                case TaskStatusType.NeedsReview:
                    if (!task.ReviewingUserGuid.HasValue)
                        throw new ValidationException("This task isn't being reviewed by manager.");

                    if (_principal.IsWorker() || _principal.IsVendor())
                        throw new ForbiddenException("You don't have rigths to review this task.");

                    if (task.ReviewingUserGuid.Value != userId)
                        throw new ForbiddenException("This task is being reviewed by another manager.");

                    switch (newStatus)
                    {
                        case TaskStatusType.InProgress:
                            task.Status = newStatus;
                            await _taskNeo4JRepository.ChangeTaskStatusAsync(task.Id, newStatus);
                            await _taskHistoryService.Write(task.Id, ActivityType.DeclineReview, selectedConditionOptionId, trx);
                            await _notificationService.SendDeclineReviewTaskAsync(trx, task.Id);

                            var statistic = await trx.Track<FormulaTaskStatistic>()
                                .Where(w => w.VendorGuid == task.ProccessingUserGuid && w.ProjectTaskId == task.Id &&
                                    w.FormulaTaskId == task.FormulaTaskId && w.Type == StatisticType.Working)
                                .OrderBy(o => o.Id).FirstOrDefaultAsync();
                            if (statistic != null)
                            {
                                trx.Add(new FormulaTaskStatistic
                                {
                                    VendorGuid = task.ProccessingUserGuid.Value,
                                    Created = _dateTimeService.NowUtc,
                                    FormulaTaskId = task.FormulaTaskId.Value,
                                    ProjectTaskId = task.Id,
                                    Type = StatisticType.Working,
                                    FormulaTaskStatisticId = statistic.FormulaTaskStatisticId ?? statistic.Id
                                });
                            }
                            break;

                        case TaskStatusType.Completed:
                            // Check if all todos are checked
                            var reviewerTodo = await trx.Read<ProjectTaskChecklist>()
                                .Where(t => t.ProjectTaskId == task.Id && !t.ReviewerIsChecked && t.Type == TodoType.Reviewer)
                                .FirstOrDefaultAsync();

                            if (reviewerTodo != null)
                                throw new ValidationException("Reviewer checklist items are pending.");

                            await CompleteTask(trx, task,
                                selectedConditionOptionId,
                                ActivityType.AcceptReview);
                            await _notificationService.SendApproveReviewTaskAsync(trx, task.Id);
                            break;

                        default:
                            throw new ValidationException("Cannot change to upcomming status manually.");
                    }

                    break;
                case TaskStatusType.New:

                    if (newStatus == TaskStatusType.InProgress &&
                        await _taskNeo4JRepository.IsAvailableToStartAsync(task.Id))
                    {
                        if (task.ProjectTaskVendors.Any(w => w.Status == ProjectRequestStatus.Accepted && w.VendorGuid == task.ProccessingUserGuid))
                        {
                            if (task.ParentTasks.Any(w => w.ParentTask.Status != TaskStatusType.Completed))
                                throw new ValidationException("Can't change task status. This task depends on some uncompleted task(s).");

                            trx.Add(new FormulaTaskStatistic
                            {
                                Created = _dateTimeService.NowUtc,
                                FormulaTaskId = task.FormulaTaskId.Value,
                                ProjectTaskId = task.Id,
                                Type = StatisticType.Responding,
                                VendorGuid = task.ProccessingUserGuid.Value
                            });
                            await _notificationService.SendStartProjectTaskOutsourcesAsync(trx, task.Id);
                        }
                        else
                        {
                            var userHasSkill = task.ProjectTaskUsers
                                .Any(t => t.ProjectTaskUserType == ProjectTaskUserType.Assigned
                                          && t.UserId == userId);

                            await _taskHistoryService.Write(task.Id, ActivityType.InProgress, trx: trx);

                            await _taskNeo4JRepository.ChangeTaskStatusAsync(task.Id, TaskStatusType.InProgress);
                            task.Status = TaskStatusType.InProgress;
                            if (task.ParentTaskId.HasValue)
                            {
                                await _taskNeo4JRepository.ChangeTaskStatusAsync(task.ParentTaskId.Value, TaskStatusType.InProgress);
                            }
                            if (userHasSkill)
                            {
                                task.ProccessingUserGuid = userId;
                                await _taskHistoryService.Write(task.Id, ActivityType.Processing, trx: trx);
                            }
                            else
                            {
                                await _notificationService.SendInProgressTaskAsync(trx, task.Id);
                            }
                        }
                        await _scheduleService.RemoveJobBegin(trx, task.Id);
                    }
                    else
                    {
                        throw new ValidationException("Cannot manually change to the provided status.");
                    }
                    break;

                default:
                    throw new ValidationException("Cannot manually change to the provided status.");
            }

            await _taskSocketStore.TaskStatusChanged(task.ProjectId, task.Id, newStatus);
        }

        public async Task UpdateTaskTree(ITransactionScope trx, ProjectTask taskOld, ProjectTask task, UpdateTaskDto model, List<int> oldConditions)
        {
            if (!(await _taskNeo4JRepository.GetParentTaskIdsAsync(taskOld.Id)).Any()
                || taskOld.ParentTasks.All(t => t.ParentTask.Status == TaskStatusType.Completed))
            {
                if (taskOld.RecurrenceOptions?.Cron != task.RecurrenceOptions?.Cron
                    || taskOld.StartDate != task.StartDate)
                {
                    await _scheduleService.RemoveJob(trx, taskOld.Id);
                    await ScheduleNewTask(trx, task, task.RecurrenceOptions?.IsAsap ?? false);
                }
                else
                {
                    if (taskOld.IsAutomated)
                    {
                        if (!task.IsAutomated)
                        {
                            await _scheduleService.RemoveJobEnd(trx, task.Id);
                        }
                        else if (taskOld.Duration != task.Duration)
                        {
                            await _scheduleService.RemoveJobEnd(trx, task.Id);
                            await _scheduleService.CreateJobEnd(trx, task);
                        }
                    }
                    else
                    {
                        if (task.IsAutomated)
                        {
                            await _scheduleService.CreateJobEnd(trx, task);
                        }
                    }
                }
            }

            if (model.IsConditional)
            {
                var removedConditional = oldConditions.Where(w => model.Condition.Options.All(a => a.AssignedTaskId != w));
                var addedConditional = model.Condition.Options
                    .Where(w => w.AssignedTaskId != 0 && oldConditions.All(a => a != w.AssignedTaskId))
                    .Select(s => s.AssignedTaskId);
                var tasks = new List<Task>();
                foreach (var removeAssignedId in removedConditional)
                {
                    await _taskNeo4JRepository.RemoveTaskConditionAsync(taskOld.Id, removeAssignedId);
                    var formulaRootTasks = await _taskNeo4JRepository.GetFormulaRootTaskIdsAsync(removeAssignedId);
                    if (formulaRootTasks.Any())
                    {
                        tasks.AddRange(from formulaSubTask in await trx.Track<ProjectTask>()
                                        .Where(w => formulaRootTasks.Contains(w.Id))
                                        .ToListAsync()
                                       select ScheduleNewTask(trx, formulaSubTask));
                    }
                    tasks.Add(_scheduleService.EndTaskJob(trx, removeAssignedId));
                }

                foreach (var newAssigned in addedConditional)
                {
                    if (await _taskNeo4JRepository.IsRootAsync(newAssigned))
                    {
                        tasks.AddRange(
                            from formulaTaskId in await _taskNeo4JRepository.GetFormulaRootTaskIdsAsync(newAssigned)
                            select _scheduleService.RemoveJob(trx, formulaTaskId));
                    }
                    tasks.Add(_taskNeo4JRepository.AddTaskConditionAsync(
                        task.Condition.Options.Where(w => w.AssignedTaskId == newAssigned)
                            .Select(s => s.Id)
                            .First(),
                        taskOld.Id, newAssigned));

                    tasks.Add(_scheduleService.RemoveJob(trx, newAssigned));
                }

                await Task.WhenAll(tasks);
                var _startDateHelperService = _startDateHelperServiceFactory();
                foreach (var taskFormula in await trx.Track<ProjectTask>().Where(w => addedConditional.Contains(w.Id) && w.FormulaId.HasValue).ToArrayAsync())
                {
                    var parentIds = await _taskNeo4JRepository.GetParentTaskIdsAsync(taskFormula.Id);
                    var date = await trx.Read<ProjectTask>().Where(w => parentIds.Contains(w.Id)).Select(s => s.RecurrenceOptions == null ?
                           s.StartDate.Value.AddMinutes(s.Duration.Value) : s.RecurrenceOptions.NextOccurenceDate.Value.AddMinutes(s.Duration.Value))
                        .OrderByDescending(o => o).FirstOrDefaultAsync();
                    await _startDateHelperService.InitTasksStartDate(trx, task.ProjectId, new Formula.Dto.ProjectStartDatesDto
                    {
                        ProjectStartDateTime = date
                    }, new int[] { taskFormula.Id });
                    await trx.SaveChangesAsync();
                }
            }
        }

        private async Task CompleteTask(
           ITransactionScope trx,
           ProjectTask task,
           int? selectedConditionOptionId,
           ActivityType type = ActivityType.Completed)
        {
            if (!string.IsNullOrEmpty(task.RecurrenceOptions?.Cron)
                && task.RecurrenceOptions.RecurrenceType != FormulaTaskRecurrenceType.EndNever)
            {
                task.RecurrenceOptions.Occurrences++;
            }

            task.Status = TaskStatusType.Completed;

            await _taskHistoryService.Write(task.Id, type, selectedConditionOptionId, trx);
            await _scheduleService.RemoveJob(trx, task.Id);
            await _scheduleService.RemoveVendorTaskReviewJob(trx, task.Id);

            var processingUser = await trx.Read<User>().SingleOrDefaultAsync(t => t.Id == task.ProccessingUserGuid);

            //if (processingUser != null && await _userManager.IsInRoleAsync(processingUser, UserRoles.Vendor)) //added logic for new role CompanyWorker WRT Sprint 10B
            //if (processingUser != null && (await _userManager.IsInRoleAsync(processingUser, UserRoles.Vendor) || await _userManager.IsInRoleAsync(processingUser, UserRoles.CompanyWorker))) //added logic for new role CompanyWorker WRT Sprint 10B
            if (processingUser != null && (await _userManager.IsInRoleAsync(processingUser, UserRoles.Vendor) || await _userManager.IsInRoleAsync(processingUser, UserRoles.CompanyWorker) || await _userManager.IsInRoleAsync(processingUser, UserRoles.Company))) //added logic for new role Company WRT Sprint 10B
            {
                var ownerCredits = await trx.Track<Domain.Models.Credits.Credits>().SingleOrDefaultAsync(t => t.UserId == _principal.GetOwnerId());
                var vendorRequest = await trx.Read<ProjectTaskVendor>()
                    .SingleOrDefaultAsync(t => t.ProjectTaskId == task.Id
                    && t.VendorGuid == processingUser.Id
                    && t.Status == ProjectRequestStatus.Accepted);
                ownerCredits.TotalCredits -= vendorRequest.Price;
                ownerCredits.LastUpdate = _dateTimeService.NowUtc;

                trx.Add(new CreditLog
                {
                    Amount = vendorRequest.Price,
                    AmountWithTax = await CalculateAmountWithTax(trx, vendorRequest.Price),
                    HistoryTime = _dateTimeService.NowUtc,
                    ManagerId = _principal.GetUserId(),
                    ProjectTaskId = task.Id,
                    VendorId = vendorRequest.VendorGuid,
                    Type = Global.Enums.CreditsLogType.CompleteTask
                });
            }

            await trx.SaveChangesAsync();
            //check if formula task
            var isFormulaSubTask = task.ParentTaskId.HasValue;
            if (isFormulaSubTask)
            {
                var formulaTaskId = task.ParentTaskId.Value;
                await _taskNeo4JRepository.ChangeTaskStatusAsync(task.Id, TaskStatusType.Completed);
                if (await _taskNeo4JRepository.IsFormulaRootCompletedAsync(formulaTaskId)
                    && await _taskNeo4JRepository.IsFormulaGraphCompletedAsync(formulaTaskId))
                {
                    var parentTask = await trx.Track<ProjectTask>()
                        .Include(i => i.RecurrenceOptions)
                        .FirstAsync(w => w.Id == formulaTaskId);
                    parentTask.Status = TaskStatusType.Completed;
                    await _taskNeo4JRepository.ChangeTaskStatusAsync(formulaTaskId, TaskStatusType.Completed);
                    if (parentTask.ParentTaskId.HasValue)
                    {
                        await CompleteTask(trx, parentTask, null);
                    }
                    else
                    {
                        await FinishTask(trx, formulaTaskId);
                    }
                }
                else
                {
                    await _scheduleService.EndTaskJob(trx, task.Id);
                }
            }
            else
            {
                await _taskNeo4JRepository.ChangeTaskStatusAsync(task.Id, TaskStatusType.Completed);
                await FinishTask(trx, task.Id);
            }

        }

        private async Task FinishTask(ITransactionScope trx, int taskId)
        {
            if (await _taskNeo4JRepository.IsGraphCompleted(taskId))
            {
                await _scheduleService.ResetProjectTaskTreeStatuses(taskId);
            }
            else
            {
                await _scheduleService.EndTaskJob(trx, taskId);
            }
        }

        private async Task<decimal> CalculateAmountWithTax(ITransactionScope trx, decimal amount)
        {
            var vendorTax = await trx.Read<CreditsTax>().SingleOrDefaultAsync(t => t.Type == Global.Enums.CreditsTaxType.Vendor);
            double amountWithTax = (double)amount - vendorTax.Fee - (vendorTax.Percentage / 100 * (double)amount);
            return (decimal)amountWithTax;
        }
    }
}
