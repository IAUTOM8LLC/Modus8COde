using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace IAutoM8.Service.CommonService.Interfaces
{
    public interface IEntityFrameworkPlus
    {
        Task<int> BulkUpdateAsync<T>(IQueryable<T> query, Expression<Func<T, T>> updateFactory) where T : class;
        Task<int> BulkDeleteAsync<T>(IQueryable<T> query) where T : class;
    }
}
