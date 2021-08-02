using IAutoM8.Domain.Models.Formula.Task;
using IAutoM8.Domain.Models.Project;
using IAutoM8.Repository;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IAutoM8.Service.ProjectTasks.Dto;

namespace IAutoM8.Service.CommonService.Interfaces
{
    public interface IProjectTaskEntityImportService
    {
        Task<Dictionary<int,int>> MapFormulaTaskAsync(ITransactionScope trx,
            Project project, FormulaTask formulaTask,
            DateTime startFrom, IEnumerable<int> outsourceSkills, IEnumerable<SkillMapDto> skillMappings, int? parentTaskId,
            Func<ITransactionScope, Project, IEnumerable<FormulaTask>,
                DateTime, IEnumerable<int>, IEnumerable<SkillMapDto>, int?, (int x, int y), Task<Dictionary<int, int>>> importTasksIntoProjectAsync);

        Task<(int, int)> MapTaskAsync(ITransactionScope trx,
            Project project, FormulaTask formulaTask,
            int? parentTaskId,
            (int x, int y) positionOffset,
            DateTime startFrom,
            IEnumerable<int> outsourceSkills,
            IEnumerable<SkillMapDto> skillMappings);

        Task MapDependencyAsync(ITransactionScope trx, Project project, FormulaTask formulaTask,
            Dictionary<int, int> tasksMaps);

        Task MapConditionAsync(ITransactionScope trx, Project project, FormulaTask formulaTask,
            KeyValuePair<int, int> pair, Dictionary<int, int> tasksMaps);
    }
}
