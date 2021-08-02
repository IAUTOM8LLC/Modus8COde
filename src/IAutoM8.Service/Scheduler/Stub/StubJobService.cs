using System;
using System.Threading.Tasks;
using IAutoM8.Service.Scheduler.Interfaces;

namespace IAutoM8.Service.Scheduler.Stub
{
    public class StubJobService : IJobService
    {
        public void ForceEnd(int id)
        {
        }

        public void ManageBegin(int id)
        {
        }

        public void ManageDaySummary(Guid id)
        {
        }

        public void ManageDeadline(int id)
        {
        }

        public void ManageDelayedTaskTreeEnd(int id)
        {
        }

        public void ManageEnd(int id)
        {
        }

        public void ManageOverdue(int id)
        {
        }

        public void ManageTaskTreeEnd(int id)
        {
        }

        public void ManageVendorTaskReview(int id)
        {
        }

        public void ManageDailyToDoSummary(Guid id)
        {

        }

        public void ManageReAssignVendors()
        {
        }
    }
}
