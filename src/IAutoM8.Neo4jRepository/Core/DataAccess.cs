using System.Collections.Generic;
using System.Linq;
using IAutoM8.Neo4jRepository.Interfaces;
using Neo4j.Driver.V1;
using System.Threading.Tasks;
using ITransaction = IAutoM8.Neo4jRepository.Interfaces.ITransaction;

namespace IAutoM8.Neo4jRepository.Core
{
    public class DataAccess : IDataAccess
    {
        private readonly IDriver _driver;
        private ITransaction _transaction;

        public DataAccess(IDriver driver)
        {
            _driver = driver;
        }

        public void SetTransaction(ITransaction transaction)
        {
            _transaction = transaction;
        }

        public ITransaction BeginTransaction()
        {
            _transaction = new Transaction(_driver);
            return _transaction;
        }

        public async Task<IStatementResultCursor> RunAsync(string statement, object parameters)
        {
            if (_transaction != null && !_transaction.IsCommited)
                return await _transaction.RunAsync(statement, parameters);
            using (var session = _driver.Session())
                return await session.RunAsync(statement, parameters);
        }

        public async Task ExecuteAsync(string statement, IEnumerable<object> parameters)
        {
            await Task.WhenAll(parameters.Select(p => RunAsync(statement, p)));

        }

        public async Task<bool> AnyAsync(string statement, object parameters)
        {
            var result = await RunAsync(statement,parameters);
            return await result.FetchAsync();
        }
    }
}
