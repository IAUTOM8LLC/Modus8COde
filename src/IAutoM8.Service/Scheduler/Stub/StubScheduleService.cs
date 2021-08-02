using IAutoM8.Repository;
using System.Threading.Tasks;
using IAutoM8.Domain.Models.Project.Task;
using IAutoM8.Service.Scheduler.Interfaces;
using System;

namespace IAutoM8.Service.Scheduler.Stub
{
    public class StubScheduleService : IScheduleService
    {
        
        public Task CreateAutomatedJob(ITransactionScope trx, ProjectTask task, bool isAsap = false)
        {
            return Task.CompletedTask;
        }

        public Task CreateJobBegin(ITransactionScope trx, ProjectTask task, bool isAsap = false)
        {
            return Task.CompletedTask;
        }

        public Task CreateJobEnd(ITransactionScope trx, ProjectTask task)
        {
            return Task.CompletedTask;
        }

        public Task RemoveJob(ITransactionScope trx, int taskId)
        {
            return Task.CompletedTask;
        }

        public Task RemoveJobEnd(ITransactionScope trx, int taskId)
        {
            return Task.CompletedTask;
        }

        public Task RemoveJobBegin(ITransactionScope trx, int taskId)
        {
            return Task.CompletedTask;
        }

        public Task ResetProjectTaskTreeStatuses(int id)
        {
            return Task.CompletedTask;
        }

        public Task EnqueneJobEnd(ITransactionScope trx, ProjectTask task)
        {
            return Task.CompletedTask;
        }

        public Task DelayedProjectTaskTreeStatuses(int id, TimeSpan delay)
        {
            return Task.CompletedTask;
        }

        public Task DaySummary(Guid id)
        {
            return Task.CompletedTask;
        }

        public Task EndTaskJob(ITransactionScope trx, int taskId)
        {
            return Task.CompletedTask;
        }

        public Task CreateIntervalJobEnd(ITransactionScope trx, int taskId, TimeSpan delay)
        {
            return Task.CompletedTask;
        }

        public Task CreateVendorTaskReviewJob(ITransactionScope trx, int taskId)
        {
            return Task.CompletedTask;
        }

        public Task DailyToDoSummary(Guid id, DateTime? notificationTime = null)
        {
            return Task.CompletedTask;
        }

        public Task RemoveVendorTaskReviewJob(ITransactionScope trx, int taskId)
        {
            return Task.CompletedTask;
        }

        public Task ReAssignVendors()
        {
            return Task.CompletedTask;
        }
    }
}
