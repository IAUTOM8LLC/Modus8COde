using Microsoft.EntityFrameworkCore.Storage;
using System.Threading;
using System.Threading.Tasks;

namespace IAutoM8.Repository
{
    public interface ITransactionScope : IRepo
    {
        void Rollback();
        void Commit();
        Task SaveAndCommitAsync(CancellationToken cancellationToken = default(CancellationToken));
        void SaveAndCommit();
    }

    public class TransactionScope : Repo, ITransactionScope
    {
        private readonly IDbContextTransaction _trx;

        public TransactionScope(Context context) : base(new Context(context.Options))
        {
            _trx = Context.Database.BeginTransaction();
        }

        public void Rollback()
        {
            _trx.Rollback();
        }

        public void Commit()
        {
            _trx.Commit();
        }

        public Task SaveAndCommitAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            Context.SaveChangesAsync(cancellationToken).Wait(cancellationToken);
            Commit();
            return Task.CompletedTask;
        }

        public void SaveAndCommit()
        {
            Context.SaveChanges();
            Commit();
        }

        public new void Dispose()
        {
            _trx.Dispose();
            base.Dispose();
        }
    }
}
