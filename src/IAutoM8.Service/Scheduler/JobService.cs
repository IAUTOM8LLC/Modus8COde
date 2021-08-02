
using AutoMapper;
using IAutoM8.Domain.Models.Credits;
using IAutoM8.Domain.Models.Project.Task;
using IAutoM8.Domain.Models.User;
using IAutoM8.Domain.Models.Vendor;
using IAutoM8.Global.Enums;
using IAutoM8.Global.Options;
using IAutoM8.Neo4jRepository.Interfaces;
using IAutoM8.Repository;
using IAutoM8.Service.Braintree.Interfaces;
using IAutoM8.Service.CommonService.Interfaces;
using IAutoM8.Service.Notification.Interfaces;
using IAutoM8.Service.ProjectTasks.Interfaces;
using IAutoM8.Service.Scheduler.Interfaces;
using IAutoM8.Service.SendGrid.Interfaces;
using IAutoM8.WebSockets.Stores.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;

namespace IAutoM8.Service.Scheduler
{
    public class JobService : IJobService
    {
        private readonly IRepo _repo;
        private readonly Func<IScheduleService> _scheduleServiceFactory;
        private readonly IDateTimeService _dateTimeService;
        private readonly INotificationService _notificationService;
        private readonly ITaskNeo4jRepository _taskNeo4JRepository;
        private readonly ITaskSocketStore _taskSocketStore;
        private readonly ICreditsService _creditsService;
        private readonly Func<ITaskService> _taskServiceFactory;
        private readonly ISendGridService _sendGridService;
        private readonly EmailTemplates _emailTemplates;//Commented

        public JobService(IRepo repo,
            Func<IScheduleService> scheduleServiceFactory,
            IDateTimeService dateTimeService,
            INotificationService notificationService,
            ITaskNeo4jRepository taskNeo4JRepository,
            ICreditsService creditsService,
            IOptions<EmailTemplates> emailTemplates,
            Func<ITaskService> taskServiceFactory,
            ISendGridService sendGridService,
            ITaskSocketStore taskSocketStore)
        {
            _repo = repo;
            _scheduleServiceFactory = scheduleServiceFactory;
            _dateTimeService = dateTimeService;
            _notificationService = notificationService;
            _taskNeo4JRepository = taskNeo4JRepository;
            _taskSocketStore = taskSocketStore;
            _taskServiceFactory = taskServiceFactory;
            _creditsService = creditsService;
            _sendGridService = sendGridService;
            _emailTemplates = emailTemplates.Value;
            _taskServiceFactory = taskServiceFactory;
        }

        public void ForceEnd(int id)
        {
            var scheduleService = _scheduleServiceFactory();
            using (var transaction = _taskNeo4JRepository.BeginTransaction())
            using (var trx = _repo.Transaction())
            {
                var task = trx
                    .Track<ProjectTask>()
                    .Include(i => i.TaskJobs)
                    .Include(i => i.Condition)
                    .ThenInclude(i => i.Options)
                    .First(w => w.Id == id);

                trx.RemoveRange(task.TaskJobs.Where(w => w.Type == TaskJobType.End));
                task.Status = TaskStatusType.Completed;
                if (task.Condition?.Options.Any(a => a.IsSelected && a.AssignedTaskId.HasValue) == true)
                {
                    var selectedOption = task.Condition.Options.First(a => a.IsSelected);
                    _taskNeo4JRepository.SetTaskConditionSelectedAsync(task.Id,
                        selectedOption.AssignedTaskId.Value).Wait();
                }
                _taskNeo4JRepository.ChangeTaskStatusAsync(id, TaskStatusType.Completed).Wait();
                trx.SaveChanges();
                if (_taskNeo4JRepository.IsGraphCompleted(id).Result)
                {
                    scheduleService.ResetProjectTaskTreeStatuses(id).Wait();
                }
                else
                {
                    scheduleService.EndTaskJob(trx, id).Wait();
                }
                trx.SaveAndCommit();
                transaction.Commit();

                _taskSocketStore.TaskStatusChanged(task.ProjectId, task.Id, TaskStatusType.Completed);
            }
        }

        public void ManageBegin(int id)
        {
            using (var transaction = _taskNeo4JRepository.BeginTransaction())
            using (var trx = _repo.Transaction())
            {
                var task = trx
                    .Track<ProjectTask>()
                    .Include(i => i.TaskJobs)
                    .Include(i => i.Project)
                    .Include(i => i.RecurrenceOptions)
                    .Include(i => i.ProjectTaskVendors)
                        .ThenInclude(i => i.Vendor)
                    .Include(i => i.ProjectTaskUsers)
                        .ThenInclude(i => i.User)
                    .First(w => w.Id == id);

                _taskNeo4JRepository.ChangeTaskStatusAsync(id, TaskStatusType.InProgress).Wait();
                task.Status = TaskStatusType.InProgress;

                var now = _dateTimeService.NowUtc;

                trx.Add(new TaskHistory
                {
                    TaskId = id,
                    HistoryTime = now,
                    Type = ActivityType.InProgress
                });
                trx.RemoveRange(task.TaskJobs.Where(w => w.Type == TaskJobType.Begin));
                if (task.ParentTaskId.HasValue)
                {
                    var parentTask = trx
                       .Track<ProjectTask>()
                       .Include(i => i.TaskJobs)
                       .First(w => w.Id == task.ParentTaskId.Value);
                    parentTask.Status = TaskStatusType.InProgress;
                    _taskNeo4JRepository.ChangeTaskStatusAsync(task.ParentTaskId.Value, TaskStatusType.InProgress).Wait();
                }
                //if (task.ProjectTaskVendors.Any(a => a.Status == ProjectRequestStatus.Accepted)) //original
                if (task.ProjectTaskVendors.Any(a => a.Status == ProjectRequestStatus.Accepted && task.RecurrenceOptions.Occurrences > 0)) //newly added
                {
                    trx.Add(new FormulaTaskStatistic
                    {
                        Created = now,
                        FormulaTaskId = task.FormulaTaskId.Value,
                        ProjectTaskId = task.Id,
                        Type = StatisticType.Working,
                        VendorGuid = task.ProccessingUserGuid.Value
                    });
                    _notificationService.SendStartProjectTaskOutsourcesAsync(trx, task.Id).Wait();
                }
                else
                {
                    // As per Client comments, commenting the outsourcer job start notification
                    // same is sent during the Assign Skills
                    //if (task.ProjectTaskVendors.Count > 0)
                    //    _notificationService.SendInviteOutsourceAsync(task).Wait();
                    //else
                    //    _notificationService.SendInProgressTaskAsync(trx, id).Wait();
                    _notificationService.SendInProgressTaskAsync(trx, id).Wait();
                }
                trx.SaveAndCommit();
                transaction.Commit();

                _taskSocketStore.TaskStatusChanged(task.ProjectId, task.Id, TaskStatusType.InProgress);
            }
        }

        public void ManageEnd(int id)
        {
            using (var transaction = _taskNeo4JRepository.BeginTransaction())
            using (var trx = _repo.Transaction())
            {
                var task = trx
                    .Track<ProjectTask>()
                    .Include(t => t.ProjectTaskUsers)
                    .Include(i => i.TaskJobs)
                    .First(w => w.Id == id);

                if (WriteOverdueHistory(trx, task))
                    return;

                var isNeedReview = WriteHistory(trx, task);

                UpdateOccurenceCount(task);
                trx.RemoveRange(task.TaskJobs.Where(w => w.Type == TaskJobType.End));

                trx.SaveChanges();

                CompleteTask(trx, task);

                trx.SaveAndCommit();
                transaction.Commit();
                if (isNeedReview)
                {
                    _notificationService.SendNeedReviewTaskAsync(trx, id).Wait();
                }

                _taskSocketStore.TaskStatusChanged(task.ProjectId, task.Id, task.Status);
            }
        }

        public void ManageTaskTreeEnd(int id)
        {
            List<int> rootTaskIds;
            var allTasks = new List<ProjectTask>();

            using (var transaction = _taskNeo4JRepository.BeginTransaction())
            using (var trx = _repo.Transaction())
            {
                var now = _dateTimeService.NowUtc;
                rootTaskIds = _taskNeo4JRepository.GetRootTaskIdsAsync(id).Result.ToList();
                if (_taskNeo4JRepository.IsRootAsync(id).Result)
                    rootTaskIds.Add(id);
                foreach (var rootaskId in rootTaskIds)
                {
                    var rootTask = trx
                        .Track<ProjectTask>()
                        .Include(i => i.RecurrenceOptions)
                        .Include(i => i.Project)
                        .Include(i => i.ProjectTaskVendors)
                            .ThenInclude(i => i.Vendor)
                        .First(w => w.Id == rootaskId);

                    var isRootTaskRecurrent = rootTask.RecurrenceOptionsId.HasValue
                                              && (rootTask.RecurrenceOptions.RecurrenceType == FormulaTaskRecurrenceType.EndNever
                                                || (rootTask.RecurrenceOptions.RecurrenceType == FormulaTaskRecurrenceType.EndAfterCertainAmount &&
                                                    rootTask.RecurrenceOptions.Occurrences <
                                                    rootTask.RecurrenceOptions.MaxOccurrences)
                                                || (rootTask.RecurrenceOptions.RecurrenceType == FormulaTaskRecurrenceType.EndOnDate ||
                                                    rootTask.RecurrenceOptions.NextOccurenceDate <
                                                    rootTask.RecurrenceOptions.EndRecurrenceDate));

                    SetNextOccurences(trx, rootTask, now, allTasks, isRootTaskRecurrent);
                }
                if (allTasks.Any(all => all.RecurrenceOptions != null
                    && IsAvailableNextReccurenceOccurence(all, all.RecurrenceOptions.NextOccurenceDate.Value)))
                {
                    allTasks.ForEach(task =>
                    {
                        task.ReviewingUserGuid = null;
                        task.ProccessingUserGuid = null;
                        task.Status = TaskStatusType.New;
                        _taskNeo4JRepository.ChangeTaskStatusAsync(task.Id, TaskStatusType.New).Wait();
                        trx.Add(new TaskHistory
                        {
                            TaskId = task.Id,
                            HistoryTime = now,
                            Type = ActivityType.New
                        });

                        if (task.RecurrenceOptions != null)
                        {
                            var owner = trx.Read<User>()
                                .Include(t => t.Credits)
                                .FirstOrDefault(t => t.Id == task.Project.OwnerGuid);

                            var reservedRequests = _creditsService.GetReservedRequests(owner.Id).Result;

                            var acceptedVendorRequest = task.ProjectTaskVendors.SingleOrDefault(t => t.Status == ProjectRequestStatus.Accepted);

                            if (acceptedVendorRequest != null)
                            {
                                if (owner.Credits.TotalCredits - reservedRequests.reservedAmount < 0)
                                {
                                    StopOutsource(task.Id, trx);
                                    var notificationText = $"Insufficient funds to continue working on task {task.Title}. Task can no longer be outsorced";
                                    _sendGridService.SendMessage(owner.Email, _emailTemplates.EmailNotification, "Task - Outsource is stopped",
                                        new Dictionary<string, string> { { "{{NotificationText}}", $"[Start Task] {task.Title}" } }).Wait();
                                }
                                else
                                {
                                    task.ProccessingUserGuid = acceptedVendorRequest.VendorGuid;
                                }
                            }
                        }
                    });
                    trx.SaveChanges();
                }
                else
                {
                    rootTaskIds.Clear();
                }

                trx.SaveAndCommit();
                transaction.Commit();
            }

            if (rootTaskIds.Any())
            {
                using (var trx = _repo.Transaction())
                {
                    var scheduleService = _scheduleServiceFactory();
                    var tasks = trx.Track<ProjectTask>()
                            .Include(i => i.RecurrenceOptions)
                            .Where(w => rootTaskIds.Contains(w.Id))
                            .ToList();

                    foreach (var task in tasks)
                    {
                        if (task.RecurrenceOptions == null)
                        {
                            if (task.FormulaId.HasValue)
                            {
                                var formulaRootTaskIds = _taskNeo4JRepository.GetFormulaRootTaskIdsAsync(task.Id).Result;
                                var formulaRootTasks = trx.Track<ProjectTask>()
                                        .Include(i => i.RecurrenceOptions)
                                        .Where(w => formulaRootTaskIds.Contains(w.Id))
                                        .ToList();
                                foreach (var formulaRootTask in formulaRootTasks)
                                {
                                    SetJobs(trx, scheduleService, formulaRootTask);
                                }
                            }
                            else
                            {
                                scheduleService.EnqueneJobEnd(trx, task).Wait();
                            }
                        }
                        else
                        {
                            SetJobs(trx, scheduleService, task);
                        }
                    }
                }
            }

            if (allTasks.Any())
            {
                var projectId = allTasks.First().ProjectId;
                _taskSocketStore.TaskStatusChanged(projectId, allTasks.ToDictionary(
                    key => key.Id,
                    value => value.Status
                ));
            }
        }

        public void ManageDelayedTaskTreeEnd(int id)
        {
            using (var trx = _repo.Transaction())
            {
                var now = _dateTimeService.NowUtc;
                RecursiveResetForConditionalTask(trx, id, now);
                // TODO: _taskSocketStore.TaskStatusChanged()
                trx.SaveAndCommit();
            }
        }

        public void ManageOverdue(int id)
        {
            using (var trx = _repo.Transaction())
            {
                var task = trx
                    .Track<ProjectTask>()
                    .Include(i => i.TaskJobs)
                    .First(w => w.Id == id);

                trx.Add(new TaskHistory
                {
                    TaskId = id,
                    HistoryTime = _dateTimeService.NowUtc,
                    Type = ActivityType.Overdue
                });
                trx.RemoveRange(task.TaskJobs.Where(w => w.Type == TaskJobType.Overdue));

                trx.SaveAndCommit();
                _notificationService.SendOverdueTaskAsync(trx, id).Wait();
            }
        }

        public void ManageDeadline(int id)
        {
            using (var trx = _repo.Transaction())
            {
                var task = trx
                    .Track<ProjectTask>()
                    .Include(i => i.TaskJobs)
                    .First(w => w.Id == id);

                trx.Add(new TaskHistory
                {
                    TaskId = id,
                    HistoryTime = _dateTimeService.NowUtc,
                    Type = ActivityType.Deadline
                });
                trx.RemoveRange(task.TaskJobs.Where(w => w.Type == TaskJobType.Deadline));

                trx.SaveAndCommit();
                _notificationService.SendDeadlineAsync(trx, id).Wait();
            }
        }

        public void ManageVendorTaskReview(int id)
        {
            using (var transaction = _taskNeo4JRepository.BeginTransaction())
            using (var trx = _repo.Transaction())
            {
                var task = trx
                    .Track<ProjectTask>()
                    .Include(i => i.TaskJobs)
                    .Include(i => i.Project)
                    .Include(i => i.ProjectTaskVendors)
                        .ThenInclude(i => i.Vendor)
                    .Include(i => i.ProjectTaskUsers)
                        .ThenInclude(i => i.User)
                    .First(w => w.Id == id);


                var todoChecklists = _repo
                    .Read<ProjectTaskChecklist>()
                    .Where(t => t.ProjectTaskId == id)
                    .ToList();

                foreach (var todo in todoChecklists)
                {
                    var todoEntity = trx
                        .Track<ProjectTaskChecklist>()
                        .SingleOrDefault(c => c.Id == todo.Id);

                    todoEntity.ReviewerIsChecked = true;

                    //trx.SaveChanges();
                }

                _taskNeo4JRepository.ChangeTaskStatusAsync(id, TaskStatusType.Completed).Wait();
                task.Status = TaskStatusType.Completed;

                var now = _dateTimeService.NowUtc;

                trx.Add(new TaskHistory
                {
                    TaskId = id,
                    HistoryTime = now,
                    Type = ActivityType.Completed
                });

                // No Idea about the parent task statuses

                trx.SaveAndCommit();
                transaction.Commit();

                _taskSocketStore.TaskStatusChanged(task.ProjectId, task.Id, TaskStatusType.Completed);
            }
        }

        public void ManageDaySummary(Guid id)
        {
            _notificationService.SendSummaryAsync(id).Wait();
        }

        public void ManageDailyToDoSummary(Guid id)
        {
            _notificationService.SendDailyToDoSummary(id).Wait();
        }

        public void ManageReAssignVendors()
        {
            _repo.ExecuteSqlCommand("[dbo].[uspReAssignVendorTasksforAllVendors]", null);
        }

        private void UpdateOccurenceCount(ProjectTask task)
        {
            if (!string.IsNullOrEmpty(task.RecurrenceOptions?.Cron)
                && task.RecurrenceOptions.RecurrenceType != FormulaTaskRecurrenceType.EndNever)
            {
                task.RecurrenceOptions.Occurrences++;
            }
        }

        private void CompleteTask(ITransactionScope trx, ProjectTask task)
        {
            var scheduleService = _scheduleServiceFactory();
            //check if formula task
            var isFormulaSubTask = task.ParentTaskId.HasValue;
            if (isFormulaSubTask)
            {
                var formulaTaskId = task.ParentTaskId.Value;
                if (_taskNeo4JRepository.IsFormulaRootCompletedAsync(formulaTaskId).Result &&
                    _taskNeo4JRepository.IsFormulaGraphCompletedAsync(formulaTaskId).Result)
                {
                    var parentTask = trx.Track<ProjectTask>()
                        .Include(i => i.RecurrenceOptions)
                        .First(w => w.Id == formulaTaskId);
                    parentTask.Status = TaskStatusType.Completed;
                    _taskNeo4JRepository.ChangeTaskStatusAsync(formulaTaskId, TaskStatusType.Completed).Wait();
                    if (parentTask.ParentTaskId.HasValue)
                    {
                        CompleteTask(trx, parentTask);
                    }
                    else
                    {
                        if (_taskNeo4JRepository.IsGraphCompleted(formulaTaskId).Result)
                        {
                            scheduleService.ResetProjectTaskTreeStatuses(formulaTaskId).Wait();
                        }
                        else
                        {
                            scheduleService.EndTaskJob(trx, formulaTaskId).Wait();
                        }
                    }
                }
                else
                {
                    scheduleService.EndTaskJob(trx, task.Id).Wait();
                }
            }
            else
            {
                if (_taskNeo4JRepository.IsGraphCompleted(task.Id).Result)
                {
                    scheduleService.ResetProjectTaskTreeStatuses(task.Id).Wait();
                }
                else
                {
                    scheduleService.EndTaskJob(trx, task.Id).Wait();
                }
            }
        }

        private bool WriteHistory(ITransactionScope trx, ProjectTask task)
        {
            var isNeedReview = false;

            var taskHistoryEntity = new TaskHistory
            {
                TaskId = task.Id,
                HistoryTime = _dateTimeService.NowUtc
            };

            if (task.ProjectTaskUsers.Any(t => t.ProjectTaskUserType == ProjectTaskUserType.Reviewing))
            {
                isNeedReview = true;
                task.Status = TaskStatusType.NeedsReview;
                taskHistoryEntity.Type = ActivityType.NeedsReview;
            }
            else
            {
                task.Status = TaskStatusType.Completed;
                taskHistoryEntity.Type = ActivityType.Completed;
            }
            _taskNeo4JRepository.ChangeTaskStatusAsync(task.Id, task.Status).Wait();
            if (!task.IsInterval)
                trx.Add(taskHistoryEntity);
            return isNeedReview;
        }

        private bool WriteOverdueHistory(ITransactionScope trx, ProjectTask task)
        {
            if (task.ProccessingUserGuid.HasValue || task.IsInterval)
                return false;

            trx.Add(new TaskHistory
            {

                HistoryTime = _dateTimeService.NowUtc,
                TaskId = task.Id,
                Type = ActivityType.Overdue
            });
            _notificationService.SendOverdueTaskAsync(trx, task.Id).Wait();
            trx.SaveAndCommit();

            return true;
        }

        private void SetJobs(ITransactionScope trx, IScheduleService scheduleService, ProjectTask task)
        {
            if (task.ParentTaskId.HasValue || IsAvailableNextReccurenceOccurence(task, task.RecurrenceOptions.NextOccurenceDate.Value))
            {
                if (task.FormulaId.HasValue)
                {
                    var formulaRootTaskIds = _taskNeo4JRepository.GetFormulaRootTaskIdsAsync(task.Id).Result;
                    var formulaRootTasks = trx.Track<ProjectTask>()
                            .Include(i => i.RecurrenceOptions)
                            .Where(w => formulaRootTaskIds.Contains(w.Id));
                    foreach (var formulaRootTask in formulaRootTasks)
                    {
                        SetJobs(trx, scheduleService, formulaRootTask);
                    }
                }
                else
                {
                    if (task.IsAutomated)
                    {
                        scheduleService.CreateAutomatedJob(trx, task).Wait();
                    }
                    else
                    {
                        scheduleService.CreateJobBegin(trx, task).Wait();
                    }
                }
            }
            else
            {
                scheduleService.EnqueneJobEnd(trx, task).Wait();
            }
        }

        private void StopOutsource(int taskId, ITransactionScope trx)
        {
            var vendorNotification = trx.Track<ProjectTaskVendor>()
            .Include(t => t.ProjectTask)
                .ThenInclude(t => t.Project)
            .Include(t => t.Vendor)
            .FirstOrDefault(t => t.ProjectTaskId == taskId && t.Status == ProjectRequestStatus.Accepted);

            if (vendorNotification == null)
            {
                throw new ValidationException("Notification doesn't exist");
            }

            var now = _dateTimeService.NowUtc;

            var creditLog = new CreditLog
            {
                Amount = vendorNotification.Price,
                AmountWithTax = CalculateAmountWithTax(trx, vendorNotification.Price),
                HistoryTime = now,
                Type = Global.Enums.CreditsLogType.StopOutsource,
                ProjectTaskId = taskId
            };

            vendorNotification.Status = ProjectRequestStatus.DeclinedByOwner;
            vendorNotification.LastModified = now;
            creditLog.ManagerId = vendorNotification.ProjectTask.Project.OwnerGuid;
            creditLog.VendorId = vendorNotification.VendorGuid;

            vendorNotification.ProjectTask.ProccessingUserGuid = null;

            trx.Add(creditLog);
        }

        private decimal CalculateAmountWithTax(ITransactionScope trx, decimal amount)
        {
            var vendorTax = trx.Read<CreditsTax>().FirstOrDefault(t => t.Type == Global.Enums.CreditsTaxType.Vendor);
            double amountWithTax = (double)amount - vendorTax.Fee - (vendorTax.Percentage / 100 * (double)amount);
            return (decimal)amountWithTax;
        }

        private void SetNextOccurences(ITransactionScope trx, ProjectTask task, DateTime now, List<ProjectTask> allTasks, bool isParentRecurrent = false)
        {
            var isFormulaTask = task.FormulaId.HasValue;
            var nextTaskIds = new List<int>();
            var time = now;
            if (allTasks.All(t => t.Id != task.Id))
                allTasks.Add(task);
            if (isFormulaTask)
            {
                time = GetLastExecutedTime(task.Id, now, allTasks);
                nextTaskIds.AddRange(_taskNeo4JRepository.GetFormulaRootTaskIdsAsync(task.Id).Result);
            }
            else
            {
                var isFormulaSubTask = task.ParentTaskId.HasValue;
                if (task.RecurrenceOptionsId.HasValue)
                {
                    task.RecurrenceOptions.IsAsap = false;
                    var nextRepeat = _dateTimeService.GetNextOccurence(task.RecurrenceOptions.Cron, GetLastExecutedTime(task.Id, now, allTasks));
                    if (isFormulaSubTask || IsAvailableNextReccurenceOccurence(task, nextRepeat))
                    {
                        task.RecurrenceOptions.NextOccurenceDate = nextRepeat;
                    }
                }
                else
                {
                    if (isFormulaSubTask || isParentRecurrent)
                    {
                        task.StartDate = GetLastExecutedTime(task.Id, now, allTasks);
                    }
                }
                if (isFormulaSubTask)
                {
                    if (_taskNeo4JRepository.IsLeafAsync(task.Id).Result)
                    {
                        var date = allTasks.Where(w => w.ParentTaskId == task.ParentTaskId)
                            .Select(s => s.RecurrenceOptions?.NextOccurenceDate.Value.AddMinutes(s.Duration.Value) ?? s.StartDate.Value.AddMinutes(s.Duration.Value))
                            .OrderByDescending(o => o).FirstOrDefault();
                        var formulaTask = allTasks.First(w => w.Id == task.ParentTaskId.Value);
                        formulaTask.Duration = (int)(date - (task.RecurrenceOptions?.NextOccurenceDate ?? task.StartDate.Value)).TotalMinutes;
                    }
                    else if (_taskNeo4JRepository.IsRootAsync(task.Id).Result)
                    {
                        var date = allTasks.Where(w => w.ParentTaskId == task.ParentTaskId)
                            .Select(s => s.RecurrenceOptions?.NextOccurenceDate ?? s.StartDate.Value)
                            .OrderBy(o => o).FirstOrDefault();
                        var formulaTask = allTasks.First(w => w.Id == task.ParentTaskId.Value);
                        formulaTask.StartDate = date;
                    }
                }
                trx.SaveChanges();
                if (task.TaskConditionId.HasValue && (isFormulaSubTask ||
                        (task.RecurrenceOptionsId.HasValue &&
                        IsAvailableNextReccurenceOccurence(task, task.RecurrenceOptions.NextOccurenceDate.Value))))
                {
                    _taskNeo4JRepository.DeselectTaskConditionsAsync(task.Id).Wait();
                    trx.Track<ProjectTaskConditionOption>()
                        .Where(w => w.TaskConditionId == task.TaskConditionId.Value && w.AssignedTaskId.HasValue).ToList()
                    .ForEach(conditionalTask =>
                    {
                        conditionalTask.IsSelected = false;
                    });
                }
            }
            nextTaskIds.AddRange(_taskNeo4JRepository.GetChildTaskIdsAsync(task.Id).Result);
            foreach (var childTaskId in nextTaskIds)
            {
                var childTask = trx
                    .Track<ProjectTask>()
                    .Include(i => i.RecurrenceOptions)
                    .Include(i => i.ProjectTaskVendors)
                            .ThenInclude(i => i.Vendor)
                    .First(w => w.Id == childTaskId);
                SetNextOccurences(trx, childTask, time, allTasks, isParentRecurrent);
            }
        }

        private DateTime GetLastExecutedTime(int taskId, DateTime now, List<ProjectTask> allTasks)
        {
            var taskIds = _taskNeo4JRepository.GetParentTaskIdsAsync(taskId).Result;
            return _dateTimeService.MaxDate(allTasks
                .Where(x => taskIds.Contains(x.Id))
                .Select(s => s.RecurrenceOptionsId.HasValue ? s.RecurrenceOptions.NextOccurenceDate.Value.AddMinutes(s.Duration.Value) : s.StartDate.Value.AddMinutes(s.Duration.Value))
                .OrderByDescending(o => o)
                .FirstOrDefault(), now);
        }

        private void RecursiveResetForConditionalTask(ITransactionScope trx, int treeId, DateTime now)
        {
            //var treeTasks = trx.Track<ProjectTask>()
            //    .Where(w => w.TreeDetailId == treeId)
            //    .ToList();
            //treeTasks.ForEach(task =>
            //{
            //    task.ReviewingUserGuid = null;
            //    task.ProccessingUserGuid = null;
            //    task.Status = TaskStatusType.New;
            //    trx.Add(new TaskHistory
            //    {
            //        TaskId = task.Id,
            //        HistoryTime = now,
            //        Type = ActivityType.New
            //    });
            //});
            //trx.Read<TreeLeaf>()
            //    .Include(i => i.Task)
            //        .ThenInclude(i => i.Condition)
            //            .ThenInclude(i => i.Options)
            //                .ThenInclude(i => i.AssignedTask)
            //    .Where(w => w.TreeDetailId == treeId && w.Task.TaskConditionId.HasValue)
            //    .SelectMany(sm => sm.Task.Condition.Options.Where(w => w.AssignedTaskId.HasValue).Select(s => s.AssignedTask.TreeDetailId.Value))
            //    .GroupBy(g => g)
            //    .ToList()
            //.ForEach(treeDetailId => RecursiveResetForConditionalTask(trx, treeDetailId.Key, now));
        }

        private bool IsAvailableNextReccurenceOccurence(ProjectTask task, DateTime nextRepeat)
        {
            return task.RecurrenceOptions.RecurrenceType == FormulaTaskRecurrenceType.EndNever
                   || (task.RecurrenceOptions.RecurrenceType == FormulaTaskRecurrenceType.EndAfterCertainAmount && task.RecurrenceOptions.Occurrences < task.RecurrenceOptions.MaxOccurrences)
                   || (task.RecurrenceOptions.RecurrenceType == FormulaTaskRecurrenceType.EndOnDate && task.RecurrenceOptions.EndRecurrenceDate.Value >= nextRepeat);
        }
    }
}
