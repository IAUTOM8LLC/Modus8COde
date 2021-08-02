using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace IAutoM8.Repository
{
    public interface IRepo : IDisposable
    {
        void Add<T>(T entityToCreate) where T : class;
        void AddRange<T>(IEnumerable<T> entitiesToCreate) where T : class;
        int SaveChanges();  //Commented
        Task AddAsync<T>(T entityToCreate) where T : class;
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken));
        IQueryable<T> Read<T>(params Expression<Func<T, object>>[] includeProperties) where T : class;
        IQueryable<T> Track<T>(params Expression<Func<T, object>>[] includeProperties) where T : class;
        Task<T> FindAsync<T>(object id, CancellationToken cancellationToken = default(CancellationToken)) where T : class;
        void Remove<T>(T entityToDelete) where T : class;
        void RemoveRange(IEnumerable<object> entitiesToDelete);
        CustomTypeSqlQuery<T> ExecuteSql<T>(IMapper mapper, string sql,
            IEnumerable<SqlParameter> sqlParameterCollection) where T : class;
        ITransactionScope Transaction();
        IRepo Scope();
        void ExecuteSqlCommand(string sql, IEnumerable<SqlParameter> sqlParameterCollection);
    }

    public class Repo : IRepo, IDisposable
    {
        protected readonly Context Context;

        /// <summary>Initializes a new instance of the <see cref="T:System.Object" /> class.</summary>
        public Repo(Context context)
        {
            Context = context;
        }

        public void Add<T>(T entityToCreate) where T : class
        {
            Context.Set<T>().Add(entityToCreate);
        }

        public void AddRange<T>(IEnumerable<T> entitiesToCreate) where T : class
        {
            Context.Set<T>().AddRange(entitiesToCreate);
        }


        public int SaveChanges()
        {
            return Context.SaveChanges();
        }

        public async Task AddAsync<T>(T entityToCreate) where T : class
        {
            await Context.Set<T>().AddAsync(entityToCreate);
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return await Context.SaveChangesAsync(cancellationToken);
        }

        public IQueryable<T> Read<T>(params Expression<Func<T, object>>[] includeProperties) where T : class
        {
            var query = Context.Set<T>().AsNoTracking().AsQueryable();
            foreach (var includeProperty in includeProperties)
            {
                query = query.Include(includeProperty);
            }
            return query;
        }

        public IQueryable<T> Track<T>(params Expression<Func<T, object>>[] includeProperties) where T : class
        {
            var query = Context.Set<T>().AsQueryable();
            foreach (var includeProperty in includeProperties)
            {
                query = query.Include(includeProperty);
            }
            return query;
        }

        public async Task<T> FindAsync<T>(object id, CancellationToken cancellationToken = default(CancellationToken))
            where T : class
        {
            return await Context.FindAsync<T>(new[] { id }, cancellationToken);
        }

        public void Remove<T>(T entityToDelete) where T : class
        {
            Context.Remove(entityToDelete);
        }

        public void RemoveRange(IEnumerable<object> entitiesToDelete)
        {
            Context.RemoveRange(entitiesToDelete);
        }

        public ITransactionScope Transaction()
        {
            return new TransactionScope(Context);
        }

        public IRepo Scope()
        {
            return new Repo(new Context(Context.Options));
        }

        public void Dispose()
        {
            Context.Dispose();
        }

        public CustomTypeSqlQuery<T> ExecuteSql<T>(IMapper mapper, string sql,
            IEnumerable<SqlParameter> sqlParameterCollection) where T : class
        {
            return Context.SqlQuery<T>(mapper, sql, sqlParameterCollection);
        }

        public void ExecuteSqlCommand(string sql, IEnumerable<SqlParameter> sqlParameterCollection)
        {
            if (sqlParameterCollection == null)
            {
                Context.Database.ExecuteSqlCommand(sql);
            }
            else
            {
                List<SqlParameter> parameterList = new List<SqlParameter>();
                foreach (var sqlParameter in sqlParameterCollection)
                {
                    parameterList.Add(sqlParameter);
                }

                SqlParameter[] parameters = parameterList.ToArray();
                Context.Database.ExecuteSqlCommand(sql, parameters);
            }
        }
    }
}
