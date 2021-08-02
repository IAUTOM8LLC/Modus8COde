using IAutoM8.Domain.Models.Project.Task;
using IAutoM8.Repository;
using IAutoM8.Service.CommonDto;
using IAutoM8.Service.Formula.Dto;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IAutoM8.Service.CommonService.Interfaces
{
    public interface ITaskStartDateHelperService
    {
        Task<InitStartDateResultDto> InitTasksStartDate(ITransactionScope trx, int projectId, ProjectStartDatesDto startDates, IEnumerable<int> taskIds);
        Task UpdateStartDatesForTreeIfNeeded(ITransactionScope trx, ProjectTask parentTask, ProjectTask childTask);
    }
}
