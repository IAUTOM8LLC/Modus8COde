using System.Threading.Tasks;
using IAutoM8.Domain.Models.Project.Task;
using IAutoM8.Global.Enums;
using IAutoM8.Repository;

namespace IAutoM8.Service.ProjectTasks.Interfaces
{
    public interface IFormulaTaskJobService
    {
        Task RemoveFormulaTaskJobs(ITransactionScope trx, ProjectTask childTask, bool isFormulaTaskRoot);
        Task AddFormulaTaskJobs(ITransactionScope trx, ProjectTask task);
        Task UpdateFormulaTaskTime(ITransactionScope trx, ProjectTask task);
        Task ScheduleTaskJobs(ProjectTask task, ITransactionScope trx);
        Task TryResetProjectTaskTreeStatuses(TaskStatusType status, int parentTaskId, int childTaskId);
    }
}
