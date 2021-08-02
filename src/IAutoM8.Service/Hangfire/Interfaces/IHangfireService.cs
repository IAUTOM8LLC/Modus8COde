using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace IAutoM8.Service.Hangfire.Interfaces
{
    public interface IHangfireService
    {
        Task RunDaySummary(string recurringJobId, Guid id, Expression<Action> methodCall, string cronExpression, TimeZoneInfo timeZone = null);
        Task RunDailyToDoSummary(string recurringJobId, Guid id, Expression<Action> methodCall, string cronExpression, TimeZoneInfo timeZone = null);
        Task RunReAssignVendors(string recurringJobId, Expression<Action> methodCall, string cronExpression, TimeZoneInfo timeZone = null);
        Task EnqueueJobManageTaskTreeEnd(int id);
        Task RunManageDelayedTaskTreeEnd(int id, TimeSpan delay);
        void DeleteJob(string jobId);
        string ScheduleJob(Expression<Action> methodCall, TimeSpan delay);
        string EnqueueJob(Expression<Action> methodCall);
        void AddOrUpdateJob(string recurringJobId, Expression<Action> methodCall, string cronExpression, TimeZoneInfo timeZone = null);
        string ScheduleJobForManageBegin(int taskId, TimeSpan timeOffset);
        string ScheduleJobForManageOverdue(int taskId, TimeSpan timeOffset);
        string ScheduleJobForManageDeadline(int taskId, TimeSpan timeOffset);
        string ScheduleJobForManageEnd(int taskId, TimeSpan timeOffset);
        string ScheduleJobForVendorTaskReview(int taskId, TimeSpan timeOffset);
        string EnqueueJobForceEnd(int taskId);
    }
}
