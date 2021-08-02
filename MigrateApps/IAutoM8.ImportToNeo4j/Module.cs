using IAutoM8.Global.Options;
using Microsoft.Extensions.DependencyInjection;

namespace IAutoM8.ImportToNeo4j
{
    public static class Module
    {
        public static void Configure(IServiceCollection services)
        {
            services.AddOptions();
            services.Configure<DbOptions>(opt =>
            {
                opt.ConnectionString = "User ID=dev;Password=dev_login;Initial Catalog=IAutoM8;Data Source=dev";
            });
            services.Configure<Neo4jOptions>(opt =>
            {
                opt.Server = "bolt://localhost:7687";
                opt.UserName = "neo4j";
                opt.Password = "111";
            });
            Neo4jRepository.Module.Configure(services);
            Repository.Infrastructure.Module.Configure(services);
            services.AddScoped<IWorker, Worker>();
        }
    }
}
