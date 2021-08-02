using System;
using System.Threading.Tasks;
using Neo4j.Driver.V1;

namespace IAutoM8.Neo4jRepository.Interfaces
{
    public interface ITransaction : IDisposable
    {
        bool IsCommited { get; }
        void Commit();
        Task<IStatementResultCursor> RunAsync(string statement, object parameters);
    }
}
