using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Hangfire;
using IAutoM8.Service.Hangfire.Interfaces;
using IAutoM8.Service.Scheduler.Interfaces;

namespace IAutoM8.Service.Hangfire
{
    public class HangfireService : IHangfireService
    {
        private readonly IJobService _jobService;

        public HangfireService(IJobService jobService)
        {
            _jobService = jobService;
        }

        public async Task RunDaySummary(string recurringJobId, Guid id, Expression<Action> methodCall, string cronExpression, TimeZoneInfo timeZone = null)
        {
            await Task.Run(() => AddOrUpdateJob(string.Format(recurringJobId, id), () => _jobService.ManageDaySummary(id), "0 0 * * *", TimeZoneInfo.Utc));
        }

        public async Task RunDailyToDoSummary(string recurringJobId, Guid id, Expression<Action> methodCall, string cronExpression, TimeZoneInfo timeZone = null)
        {
            await Task.Run(() => AddOrUpdateJob(string.Format(recurringJobId, id),
                    () => _jobService.ManageDailyToDoSummary(id), cronExpression, TimeZoneInfo.Utc));
        }

        public async Task EnqueueJobManageTaskTreeEnd(int id)
        {
            await Task.Run(() => EnqueueJob(() => _jobService.ManageTaskTreeEnd(id)));
        }

        public string EnqueueJobForceEnd(int taskId)
        {
            return EnqueueJob(() => _jobService.ForceEnd(taskId));
        }

        public async Task RunManageDelayedTaskTreeEnd(int id, TimeSpan delay)
        {
            await Task.Run(() => ScheduleJob(() => _jobService.ManageDelayedTaskTreeEnd(id), delay));
        }

        public void AddOrUpdateJob(string recurringJobId, Expression<Action> methodCall, string cronExpression, TimeZoneInfo timeZone = null)
        {
            RecurringJob.AddOrUpdate(recurringJobId, methodCall, cronExpression, timeZone);
        }

        public void DeleteJob(string jobId)
        {
            BackgroundJob.Delete(jobId);
        }

        public string ScheduleJob(Expression<Action> methodCall, TimeSpan delay)
        {
            return BackgroundJob.Schedule(methodCall, delay);
        }

        public string ScheduleJobForManageBegin(int taskId, TimeSpan timeOffset)
        {
            return ScheduleJob(() => _jobService.ManageBegin(taskId), timeOffset);
        }
        public string ScheduleJobForManageOverdue(int taskId, TimeSpan timeOffset)
        {
            return ScheduleJob(() => _jobService.ManageOverdue(taskId), timeOffset);
        }

        public string ScheduleJobForManageDeadline(int taskId, TimeSpan timeOffset)
        {
            return ScheduleJob(() => _jobService.ManageDeadline(taskId), timeOffset);
        }

        public string ScheduleJobForManageEnd(int taskId, TimeSpan timeOffset)
        {
            return ScheduleJob(() => _jobService.ManageEnd(taskId), timeOffset);
        }

        public string ScheduleJobForVendorTaskReview(int taskId, TimeSpan timeOffset)
        {
            return ScheduleJob(() => _jobService.ManageVendorTaskReview(taskId), timeOffset);
        }

        public string EnqueueJob(Expression<Action> methodCall)
        {
            return BackgroundJob.Enqueue(methodCall);
        }

        public async Task RunReAssignVendors(string recurringJobId, Expression<Action> methodCall, string cronExpression, TimeZoneInfo timeZone = null)
        {
            await Task.Run(() => AddOrUpdateJob(recurringJobId, () => _jobService.ManageReAssignVendors(), cronExpression, TimeZoneInfo.Utc));
        }
    }
}
