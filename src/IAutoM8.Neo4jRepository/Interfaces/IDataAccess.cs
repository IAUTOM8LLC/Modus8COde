using System.Collections.Generic;
using Neo4j.Driver.V1;
using System.Threading.Tasks;

namespace IAutoM8.Neo4jRepository.Interfaces
{
    public interface IDataAccess
    {
        ITransaction BeginTransaction();
        void SetTransaction(ITransaction transaction);
        Task<IStatementResultCursor> RunAsync(string statement, object parameters);
        Task<bool> AnyAsync(string statement, object parameters);
        Task ExecuteAsync(string statement, IEnumerable<object> parameters);
    }
}
