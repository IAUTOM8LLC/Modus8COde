using AutoMapper;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace IAutoM8.Repository
{
    public static class DatabaseExtensions
    {
        public static CustomTypeSqlQuery<T> SqlQuery<T>(
                this Context context,
                IMapper mapper,
                string sqlQuery,
                IEnumerable<SqlParameter> parameters) where T : class
        {
            return new CustomTypeSqlQuery<T>()
            {
                DbContext = context,
                SQLQuery = sqlQuery,
                Parameters = parameters,
                Mapper = mapper
            };
        }
    }
}
