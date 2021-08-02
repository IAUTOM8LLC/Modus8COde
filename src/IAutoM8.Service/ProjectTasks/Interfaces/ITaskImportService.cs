using IAutoM8.Domain.Models.Formula.Task;
using IAutoM8.Domain.Models.Project;
using IAutoM8.Domain.Models.Project.Task;
using IAutoM8.Repository;
using IAutoM8.Service.ProjectTasks.Dto;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IAutoM8.Service.ProjectTasks.Interfaces
{
    public interface ITaskImportService
    {
        Task<Dictionary<int, int>> ImportTasksIntoProjectAsync(
            ITransactionScope trx,
            Project project,
            IEnumerable<FormulaTask> formulaTasks,
            DateTime startFrom,
            IEnumerable<int> outsourcesSkills,
            IEnumerable<SkillMapDto> skillMappings,
            int? parentTaskId = null,
            (int x, int y) positionOffset = default((int, int)));

        Task ScheduleJobsAsync(ITransactionScope trx, IEnumerable<ProjectTask> tasks);
    }
}
