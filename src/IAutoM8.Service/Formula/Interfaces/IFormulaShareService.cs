using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IAutoM8.Global.Enums;
using IAutoM8.Service.Formula.Dto;

namespace IAutoM8.Service.Formula.Interfaces
{
    public interface IFormulaShareService
    {
        Task<FormulaDto> UpdateFormulaShareStatus(int formulaId, FormulaShareStatusDto formulaShareStatusDto);
        Task<IList<FormulaUserShareDto>> GetFormulaShareList(int formulaId);
        Task<FormulaUserShareDto> ShareFormulaToUser(FormulaUserShareDto model);
        Task RemoveUserFromShareList(FormulaUserShareDto model);
        Task CopyFormulaToUser(int formulaId, Guid userGuid);
        Task<FormulaShareType> GetFormulaShareStatus(int formulaId);
        Task<List<string>> CheckFormulaAndSubformulasForSharing(int formulaId);
        Task<IList<int>> GetUniqueFormulas(IList<int> formulaIds);
    }
}
