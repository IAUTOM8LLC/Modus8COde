using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace IAutoM8.Repository
{
    public class CustomTypeSqlQuery<T> where T : class
    {
        public CustomTypeSqlQuery()
        {
        }
        public IEnumerable<SqlParameter> Parameters { get; set; }
        public Context DbContext { get; set; }
        public string SQLQuery { get; set; }
        public IMapper Mapper { get; set; }
        public async Task<IList<T>> ToListAsync()
        {
            IList<T> results = new List<T>();
            using (var conn = DbContext.Database.GetDbConnection())
            {
                await conn.OpenAsync();
                using (var command = conn.CreateCommand())
                {
                    command.CommandText = SQLQuery;
                    foreach (var param in Parameters)
                        command.Parameters.Add(param);
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (reader.HasRows)
                            results = Mapper
                                .Map<IDataReader, IEnumerable<T>>(reader)
                                .ToList();
                    }
                }
            }
            return results;
        }
    }
}
