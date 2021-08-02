using System.Threading.Tasks;
using IAutoM8.Domain.Models.Project.Task;
using IAutoM8.Repository;
using System;

namespace IAutoM8.Service.Scheduler.Interfaces
{
    public interface IScheduleService
    {
        Task DaySummary(Guid id);
        Task DailyToDoSummary(Guid id, DateTime? notificationTime = null);
        Task ReAssignVendors();
        Task EndTaskJob(ITransactionScope trx, int taskId);
        Task RemoveJob(ITransactionScope trx, int taskId);
        Task RemoveJobBegin(ITransactionScope trx, int taskId);
        Task RemoveJobEnd(ITransactionScope trx, int taskId);
        Task RemoveVendorTaskReviewJob(ITransactionScope trx, int taskId);
        Task EnqueneJobEnd(ITransactionScope trx, ProjectTask task);
        Task CreateJobEnd(ITransactionScope trx, ProjectTask task);
        Task CreateVendorTaskReviewJob(ITransactionScope trx, int taskId);
        Task CreateJobBegin(ITransactionScope trx, ProjectTask task, bool isAsap = false);
        Task CreateAutomatedJob(ITransactionScope trx, ProjectTask task, bool isAsap = false);
        Task ResetProjectTaskTreeStatuses(int id);
        Task DelayedProjectTaskTreeStatuses(int id, TimeSpan delay);
        Task CreateIntervalJobEnd(ITransactionScope trx, int taskId, TimeSpan delay);
    }
}
