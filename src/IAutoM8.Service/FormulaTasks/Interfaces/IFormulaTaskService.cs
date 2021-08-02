using IAutoM8.Domain.Models.Formula.Task;
using IAutoM8.Repository;
using IAutoM8.Service.CommonDto;
using IAutoM8.Service.FormulaTasks.Dto;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IAutoM8.Domain.Models.Skill;

namespace IAutoM8.Service.FormulaTasks.Interfaces
{
    public interface IFormulaTaskService
    {
        Task<bool> GetFormulaLockStatus(int formulaId);
        Task<List<FormulaTaskDto>> GetTasksAsync(int formulaId);
        Task<FormulaTaskDto> GetTaskAsync(int formulaTaskId, int formulaId);
        Task<List<AddFormulaTaskDisableStatusDto>> GetDisabledTasksAsync(int internalChildFormulaId, int parentFormulaId, int childFormulaId);
        Task<FormulaTaskDto> AddTaskAsync(UpdateFormulaTaskDto model);
        Task<FormulaTaskDto> AddFormulaTaskAsync(AddFormulaTaskDto model);
        Task<FormulaTaskDto> UpdateTaskAsync(UpdateFormulaTaskDto model);
        Task UpdateTasksPositionAsync(List<TaskPositionDto> list);
        Task DeleteTask(int taskId);
        Task AddTaskDependency(FormulaTaskDependencyDto model);
        Task RemoveTaskDependency(FormulaTaskDependencyDto model);
        Task DeleteTasks(ITransactionScope trx, IEnumerable<int> formulaTaskIds);
        Task CreateTaskDependency(ITransactionScope trx, FormulaTask parentTask, FormulaTask childTask, bool isRequired);
        Task AssignTaskToConditionOption(int conditionOptionId, int? taskId);
        Task<FormulaTask> CreateTask(ITransactionScope trx, FormulaTask formulaTask, Guid userGuid,
            List<Skill> bussinessSkills, int formulaId, int? internalFormulaId);
        Task<FormulaTaskCondition> CreateCondition(ITransactionScope trx, FormulaTask formulaTask, FormulaTaskCondition formulaTaskCondition);
        Task<FormulaTaskConditionOption> CreateConditionOption(ITransactionScope trx, FormulaTaskConditionOption formulaTaskConditionOption, FormulaTask optionTarget);
        Task<int> GetGroupTaskCount(int groupTaskId);
        Task CopyResources(KeyValuePair<FormulaTask, FormulaTask> pair);
        Task<List<FormulaNotesDto>> GetFormulaNotesAsync(int formulaId);
        Task<FormulaNotesDto> AddFormulaNotesAsync(AddFormulaNotesDto model);
        Task DeleteFormulaNotesAsync(int id);
        Task SetTrainingLockStatus(int taskId, bool lockStatus);
        Task EnableFormulaTask(AddFormulaTaskDisableStatusDto model);
        Task DisableFormulaTask(AddFormulaTaskDisableStatusDto model);
        Task AddPublishedTaskToNeo4j(int formulaId);
    }
}
