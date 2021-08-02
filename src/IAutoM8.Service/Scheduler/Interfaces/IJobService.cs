using System;
using IAutoM8.Domain.Models.User;

namespace IAutoM8.Service.Scheduler.Interfaces
{
    public interface IJobService
    {
        void ManageDaySummary(Guid id);
        void ManageOverdue(int id);
        void ManageDeadline(int id);
        void ManageBegin(int id);
        void ForceEnd(int id);
        void ManageEnd(int id);
        void ManageTaskTreeEnd(int id);
        void ManageVendorTaskReview(int id);
        void ManageDelayedTaskTreeEnd(int id);
        void ManageDailyToDoSummary(Guid id);
        void ManageReAssignVendors();
    }
}
