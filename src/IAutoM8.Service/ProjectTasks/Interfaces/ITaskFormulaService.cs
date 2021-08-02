using IAutoM8.Service.ProjectTasks.Dto;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IAutoM8.Service.ProjectTasks.Interfaces
{
    public interface ITaskFormulaService
    {
        Task<IEnumerable<TaskDto>> CreateFormulaTaskAsync(TaskFormulaDto model);
    }
}
