using IAutoM8.Service.CommonService.Interfaces;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Z.EntityFramework.Plus;


namespace IAutoM8.Service.CommonService
{
    public class EntityFrameworkPlus : IEntityFrameworkPlus
    {
        public async Task<int> BulkDeleteAsync<T>(IQueryable<T> query) where T : class
        {
            return await query.DeleteAsync();
        }

        public async Task<int> BulkUpdateAsync<T>(IQueryable<T> query, Expression<Func<T, T>> updateFactory) where T : class
        {
            return await query.UpdateAsync(updateFactory);
        }
    }
}
