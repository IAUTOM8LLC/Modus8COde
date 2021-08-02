using Neo4j.Driver.V1;
using System.Threading.Tasks;
using ITransaction = IAutoM8.Neo4jRepository.Interfaces.ITransaction;

namespace IAutoM8.Neo4jRepository.Core
{
    public class Transaction : ITransaction
    {
        public bool IsCommited { get; private set; }

        private readonly ISession _session;
        private readonly Neo4j.Driver.V1.ITransaction _transaction;

        public Transaction(IDriver driver)
        {
            _session = driver.Session();
            _transaction = _session.BeginTransaction();
        }

        public void Commit()
        {
            IsCommited = true;
        }

        public void Dispose()
        {
            if (IsCommited)
                _transaction.Success();
            else
                _transaction.Failure();

            _transaction.Dispose();
            _session.Dispose();
        }

        public async Task<IStatementResultCursor> RunAsync(string statement, object parameters)
        {
            return await _transaction.RunAsync(statement, parameters);
        }
    }
}
