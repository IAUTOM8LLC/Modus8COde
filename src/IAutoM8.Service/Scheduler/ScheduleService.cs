using IAutoM8.Domain.Models.Project.Task;
using IAutoM8.Global.Enums;
using IAutoM8.Repository;
using IAutoM8.Service.Scheduler.Interfaces;
using Microsoft.EntityFrameworkCore;
using NCrontab.Advanced;
using NCrontab.Advanced.Enumerations;
using System;
using System.Linq;
using System.Threading.Tasks;
using IAutoM8.Service.CommonService.Interfaces;
using IAutoM8.Neo4jRepository.Interfaces;
using IAutoM8.Service.Hangfire.Interfaces;
using System.ComponentModel.DataAnnotations;
using Hangfire;
using Hangfire.Common;

namespace IAutoM8.Service.Scheduler
{
    public class ScheduleService : IScheduleService
    {
        private const string ReccuringBussinessJobId = "{0}-bussiness";
        private const string ReccuringSummaryJobId = "{0}-dailyToDoSummary";
        private const string ReccuringReAssignVendorId = "reAssignVendors";
        private readonly IJobService _jobService;
        private readonly IRepo _repo;
        private readonly IDateTimeService _dateTimeService;
        private readonly ITaskNeo4jRepository _taskNeo4JRepository;
        private readonly IHangfireService _hangfireService;

        public ScheduleService(IRepo repo, IJobService jobService, IDateTimeService dateTimeService,
            ITaskNeo4jRepository taskNeo4JRepository, IHangfireService hangfireService)
        {
            _repo = repo;
            _jobService = jobService;
            _dateTimeService = dateTimeService;
            _taskNeo4JRepository = taskNeo4JRepository;
            _hangfireService = hangfireService;
        }

        public async Task DaySummary(Guid id)
        {
            await _hangfireService.RunDaySummary(ReccuringBussinessJobId, id, () => _jobService.ManageDaySummary(id), "0 0 * * *", TimeZoneInfo.Utc);
        }

        public async Task DailyToDoSummary(Guid id, DateTime? notificationTime = null)
        {
            var cron = notificationTime != null ?
                $"{notificationTime.Value.Minute} {notificationTime.Value.Hour} * * *" :
                "0 8 * * *";

            await _hangfireService.RunDailyToDoSummary(ReccuringSummaryJobId, id, () => _jobService.ManageDailyToDoSummary(id), cron, TimeZoneInfo.Utc);
        }

        public async Task ReAssignVendors()
        {
            await _hangfireService.RunReAssignVendors(ReccuringReAssignVendorId, () => _jobService.ManageReAssignVendors(), "0/10 * * * *", TimeZoneInfo.Utc);
        }

        public async Task RemoveJob(ITransactionScope trx, int taskId)
        {
            var taskJobs = await trx.Track<TaskJob>().Where(w => w.TaskId == taskId).ToListAsync();
            foreach(var taskJob in taskJobs)
            {
                _hangfireService.DeleteJob(taskJob.HangfireJobId);
            }
            trx.RemoveRange(taskJobs);
            await trx.SaveChangesAsync();
        }
        public async Task RemoveJobBegin(ITransactionScope trx, int taskId)
        {
            var taskJobs = await trx.Track<TaskJob>().Where(w => w.TaskId == taskId && w.Type == TaskJobType.Begin).ToListAsync();
            foreach (var taskJob in taskJobs)
            {
                _hangfireService.DeleteJob(taskJob.HangfireJobId);
            }
            trx.RemoveRange(taskJobs);
            await trx.SaveChangesAsync();
        }
        public async Task RemoveJobEnd(ITransactionScope trx, int taskId)
        {
            var taskJobs = await trx.Track<TaskJob>().Where(w => w.TaskId == taskId && w.Type != TaskJobType.Begin).ToListAsync();
            foreach (var taskJob in taskJobs)
            {
                _hangfireService.DeleteJob(taskJob.HangfireJobId);
            }
            trx.RemoveRange(taskJobs);
            await trx.SaveChangesAsync();
        }
        public async Task RemoveVendorTaskReviewJob(ITransactionScope trx, int taskId)
        {
            var reviewJobs = await trx.Track<TaskJob>()
                .Where(w => w.TaskId == taskId && w.Type == TaskJobType.Review)
                .ToListAsync();

            foreach (var reviewJob in reviewJobs)
            {
                _hangfireService.DeleteJob(reviewJob.HangfireJobId);
            }
            trx.RemoveRange(reviewJobs);
            await trx.SaveChangesAsync();
        }
        public async Task CreateJobBegin(ITransactionScope trx, ProjectTask task, bool isAsap = false)
        {
            if (!IsValid(task))
                throw new ValidationException("Task is not valid");
            var nextOccurences = GetNextExcecutingTime(task, isAsap);
            if (!nextOccurences.HasValue)
                throw new ValidationException("Next occurence is not set or incorrect");
            var timeOffset = nextOccurences.Value;
            var id = _hangfireService.ScheduleJobForManageBegin(task.Id, timeOffset);
            var overdueId = _hangfireService.ScheduleJobForManageOverdue(task.Id, timeOffset.Add(TimeSpan.FromMinutes(task.Duration.Value)));
            var deadlineMinutes = task.Duration.Value * 0.85;
            deadlineMinutes = deadlineMinutes < 1 ? (task.Duration.Value == 1 ? 0 : 1) : deadlineMinutes;
            if (deadlineMinutes > 0)
            {
                var deadlineId = _hangfireService.ScheduleJobForManageDeadline(task.Id, timeOffset.Add(TimeSpan.FromMinutes(deadlineMinutes)));
                await trx.AddAsync(new TaskJob
                {
                    TaskId = task.Id,
                    HangfireJobId = deadlineId,
                    Type = TaskJobType.Deadline
                });
            }
            await trx.AddAsync(new TaskJob
            {
                TaskId = task.Id,
                HangfireJobId = id,
                Type = TaskJobType.Begin
            });
            await trx.AddAsync(new TaskJob
            {
                TaskId = task.Id,
                HangfireJobId = overdueId,
                Type = TaskJobType.Overdue
            });
            await trx.SaveChangesAsync();
        }

        public async Task CreateJobEnd(ITransactionScope trx, ProjectTask task)
        {
            if (!IsValid(task))
                throw new ValidationException("Task is not valid");
            var nextOccurences = GetNextExcecutingTime(task);
            if (!nextOccurences.HasValue)
                return;
            var timeOffset = nextOccurences.Value;
            var id = _hangfireService.ScheduleJobForManageEnd(task.Id, timeOffset.Add(TimeSpan.FromMinutes(task.Duration.Value)));
            await trx.AddAsync(new TaskJob
            {
                TaskId = task.Id,
                HangfireJobId = id,
                Type = TaskJobType.End
            });
            await trx.SaveChangesAsync();
        }

        public async Task CreateVendorTaskReviewJob(ITransactionScope trx, int taskId)
        {
            var now = _dateTimeService.NowUtc;
            var nextDate = _dateTimeService.NowUtc.AddHours(24); // 23-12-2020 AT_Bugs
            var timeOffset = nextDate - now;

            var reviewId = _hangfireService.ScheduleJobForVendorTaskReview(taskId, timeOffset);

            await trx.AddAsync(new TaskJob
            {
                TaskId = taskId,
                HangfireJobId = reviewId,
                Type = TaskJobType.Review
            });
            await trx.SaveChangesAsync();
        }

        public async Task CreateAutomatedJob(ITransactionScope trx, ProjectTask task, bool isAsap = false)
        {
            if (!IsValid(task))
                throw new ValidationException("Task is not valid");
            var nextOccurences = GetNextExcecutingTime(task, isAsap);
            if(!nextOccurences.HasValue)
                throw new ValidationException("Next occurence is not set");
            var timeOffset = nextOccurences.Value;
            var idBegin = _hangfireService.ScheduleJobForManageBegin(task.Id, timeOffset);
            var idEnd = _hangfireService.ScheduleJobForManageEnd(task.Id, timeOffset.Add(TimeSpan.FromMinutes(task.Duration.Value)));
            await trx.AddAsync(new TaskJob
            {
                TaskId = task.Id,
                HangfireJobId = idBegin,
                Type = TaskJobType.Begin
            });
            await trx.AddAsync(new TaskJob
            {
                TaskId = task.Id,
                HangfireJobId = idEnd,
                Type = TaskJobType.End
            });
            await trx.SaveChangesAsync();
        }

        private static bool IsValid(ProjectTask task)
        {
            return !string.IsNullOrEmpty(task.RecurrenceOptions?.Cron) || task.StartDate.HasValue;
        }

        private TimeSpan? GetNextExcecutingTime(ProjectTask task, bool isAsap = false)
        {
            var timeOffset = new TimeSpan();
            if (string.IsNullOrEmpty(task.RecurrenceOptions?.Cron))
            {
                timeOffset = task.StartDate.Value - _dateTimeService.NowUtc;
            }
            else
            {
                var now = _dateTimeService.NowUtc;
                if (isAsap)
                    timeOffset = task.RecurrenceOptions.NextOccurenceDate.Value - now;
                else
                {
                    var cronSchedule = CrontabSchedule.Parse(task.RecurrenceOptions?.Cron, CronStringFormat.WithYears);
                    var nextOccurrence = cronSchedule.GetNextOccurrence(now);
                    if (task.RecurrenceOptions.NextOccurenceDate.HasValue && nextOccurrence < task.RecurrenceOptions.NextOccurenceDate.Value)
                    {
                        timeOffset = task.RecurrenceOptions.NextOccurenceDate.Value - now;
                    }
                    else
                    {
                        task.RecurrenceOptions.NextOccurenceDate = nextOccurrence;
                        timeOffset = nextOccurrence - now;
                    }
                }
            }
            return timeOffset;
        }

        public async Task ResetProjectTaskTreeStatuses(int id)
        {
            await _hangfireService.EnqueueJobManageTaskTreeEnd(id);
        }

        public async Task DelayedProjectTaskTreeStatuses(int id, TimeSpan delay)
        {
            await _hangfireService.RunManageDelayedTaskTreeEnd(id, delay);
        }

        public async Task EnqueneJobEnd(ITransactionScope trx, ProjectTask task)
        {
            var id = _hangfireService.EnqueueJobForceEnd(task.Id);
            await trx.AddAsync(new TaskJob
            {
                TaskId = task.Id,
                HangfireJobId = id,
                Type = TaskJobType.End
            });
            await trx.SaveChangesAsync();
        }

        public async Task EndTaskJob(ITransactionScope trx, int taskId)
        {
            var readyToStart = await _taskNeo4JRepository.GetReadyToStartTaskIdsAsync(taskId);
            var tasks = await trx.Read<ProjectTask>()
                    .Include(i => i.RecurrenceOptions)
                    .Include(i => i.TaskJobs)
                    .Where(w => readyToStart.Contains(w.Id) && !w.TaskJobs.Any(a => a.Type == TaskJobType.Begin)).ToListAsync();

            var rootTaskIds = await _taskNeo4JRepository.GetRootTaskIdsAsync(taskId);

            var isRootTaskRecurrent = await trx
                        .Track<ProjectTask>()
                        .Include(i => i.RecurrenceOptions)
                        .Where(w => rootTaskIds.Contains(w.Id) && w.RecurrenceOptionsId.HasValue
                                      && (w.RecurrenceOptions.RecurrenceType == FormulaTaskRecurrenceType.EndNever
                                        || (w.RecurrenceOptions.RecurrenceType == FormulaTaskRecurrenceType.EndAfterCertainAmount &&
                                            w.RecurrenceOptions.Occurrences <
                                            w.RecurrenceOptions.MaxOccurrences)
                                        || (w.RecurrenceOptions.RecurrenceType == FormulaTaskRecurrenceType.EndOnDate ||
                                            w.RecurrenceOptions.NextOccurenceDate <
                                            w.RecurrenceOptions.EndRecurrenceDate))).AnyAsync();


            for (var i = 0; i < tasks.Count; i++)
            {
                var task = tasks[i];
                var isFormulaTask = task.FormulaId.HasValue;
                if (isFormulaTask)
                {
                    task.Status = TaskStatusType.InProgress;
                    await _taskNeo4JRepository.ChangeTaskStatusAsync(task.Id, TaskStatusType.InProgress);
                    var formulaParentTaskIds = await _taskNeo4JRepository.GetFormulaRootTaskIdsAsync(task.Id);
                    tasks.AddRange(await trx.Read<ProjectTask>()
                        .Include(t => t.RecurrenceOptions)
                        .Where(w => formulaParentTaskIds.Contains(w.Id))
                        .ToListAsync());
                }
                else
                {
                    if (task.IsInterval)
                    {
                        task.Status = TaskStatusType.InProgress;
                        await trx.SaveChangesAsync();
                        await CreateIntervalJobEnd(trx, task.Id, TimeSpan.FromMinutes(task.Duration.Value));
                    }
                    else
                    {
                        var nextRepeat = new DateTime();
                        if (task.RecurrenceOptionsId.HasValue)
                        {
                            var recurrenceDetails = _dateTimeService.ParseRecurrenceAsap(task.RecurrenceOptions, _dateTimeService.NowUtc);
                            nextRepeat = _dateTimeService.GetNextOccurence(recurrenceDetails);
                        }
                        var isFormulaSubTask = task.ParentTaskId.HasValue;
                        if (task.RecurrenceOptionsId.HasValue)
                        {
                            if (isFormulaSubTask || task.RecurrenceOptions.RecurrenceType == FormulaTaskRecurrenceType.EndNever ||
                              (task.RecurrenceOptions.RecurrenceType == FormulaTaskRecurrenceType.EndAfterCertainAmount && task.RecurrenceOptions.Occurrences < task.RecurrenceOptions.MaxOccurrences) ||
                              (task.RecurrenceOptions.RecurrenceType == FormulaTaskRecurrenceType.EndOnDate && (task.RecurrenceOptions.EndRecurrenceDate.Value >= nextRepeat || task.RecurrenceOptions.Occurrences <= 1)))
                            {
                                task.RecurrenceOptions.NextOccurenceDate = nextRepeat;
                                await trx.SaveChangesAsync();
                                if (task.IsAutomated)
                                {
                                    await CreateAutomatedJob(trx, task, task.RecurrenceOptions.IsAsap);
                                }
                                else
                                {
                                    await CreateJobBegin(trx, task, task.RecurrenceOptions.IsAsap);
                                }
                            }
                            else
                            {
                                await EnqueneJobEnd(trx, task);
                            }
                        }
                        else
                        {
                            if (isFormulaSubTask
                                || !trx.Read<TaskHistory>().Any(w => (w.Type == ActivityType.Completed || w.Type == ActivityType.AcceptReview) && w.TaskId == task.Id)
                                || isRootTaskRecurrent)
                            {
                                if (task.IsAutomated)
                                {
                                    await CreateAutomatedJob(trx, task);
                                }
                                else
                                {
                                    await CreateJobBegin(trx, task);
                                }
                            }
                            else
                            {
                                await EnqueneJobEnd(trx, task);
                            }
                        }
                    }
                }
            }
        }

        public async Task CreateIntervalJobEnd(ITransactionScope trx, int taskId, TimeSpan delay)
        {
            var id = _hangfireService.ScheduleJob(() => _jobService.ManageEnd(taskId), delay);
            await trx.AddAsync(new TaskJob
            {
                TaskId = taskId,
                HangfireJobId = id,
                Type = TaskJobType.End
            });
            await trx.SaveChangesAsync();
        }
    }
}
