using IAutoM8.Service.Formula.Dto;
using IAutoM8.Service.Teams.Dto;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IAutoM8.Service.Formula.Interfaces
{
    public interface IFormulaService
    {
        Task<SearchFormulaResultDto<FormulaListingDto>> GetFormulas(SearchFormulaDto search);
        Task<SearchFormulaResultDto<FormulaListingDto>> GetCustomFormulas(SearchFormulaDto search);
        Task<SearchFormulaResultDto<FormulaListingDto>> GetPublicFormulas(SearchFormulaDto search);
        Task<IList<AllFormulaMeanTatDto>> GetFormulaMeanTatValue(System.Guid userId, bool isGlobal);
        Task<SearchFormulaResultDto<FormulaSearchListingDto>> GetAllFormulas(SearchFormulaDto search);
        Task<List<FormulaListingDto>> GetOwnedFormulas();
        Task<FormulaDto> AddFormula(AddFormulaDto formula);
        Task<FormulaDto> UpdateFormula(FormulaDto formula);
        Task DeleteFormula(int formulaId);
        Task ImportTasksIntoProject(int projectId, ImportTasksDto model);
        Task<FormulaDto> GetFormula(int formulaId);
        Task SetLockStatus(int formulaId, bool lockStatus);
        Task SetStarredStatus(int formulaId, bool starredStatus);
        Task<FormulaDto> ChangeFormulaStatus(int formulaId);
        Task<IList<CopyFormula>> CopyFormula(int formulaId, string formulaName, string description, bool isAdmin);
        Task CopyResources(int oldTaskId, int newTaskId, int newFormulaID);
    }
}
