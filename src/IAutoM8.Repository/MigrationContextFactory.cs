using IAutoM8.Global.Options;
using Microsoft.EntityFrameworkCore.Design;
using Newtonsoft.Json;
using System;
using System.IO;

namespace IAutoM8.Repository
{
    public class MigrationContextFactory : IDesignTimeDbContextFactory<Context>
    {
        public Context CreateDbContext(string[] args)
        {
            var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            env = string.IsNullOrEmpty(env) ? "development" : env;

            var appsettings = File.ReadAllText($"../IAutoM8/appsettings.json");
            dynamic set = JsonConvert.DeserializeObject(appsettings);

            return new Context(new DbOptions
            {
                ConnectionString = set.ConnectionStrings.IAutoM8
            });
        }
    }
}
