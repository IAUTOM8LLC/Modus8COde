using System.Collections.Generic;
using System.Threading.Tasks;

namespace IAutoM8.Neo4jRepository.Interfaces
{
    public interface IFormulaNeo4jRepository
    {
        ITransaction BeginTransaction();

        Task AddFormulaAsync(int formulaId);
        Task AddRelationAsync(int id, int internalFormulaId);
        Task RemoveRelationAsync(int id, int internalFormulaId);
        Task<bool> HasLoopAsync(int id);
        Task<IEnumerable<int>> GetAllInternalFormulaIds(int id);
    }
}
