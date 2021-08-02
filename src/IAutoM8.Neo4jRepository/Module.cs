using IAutoM8.Global.Options;
using IAutoM8.Neo4jRepository.Core;
using IAutoM8.Neo4jRepository.Interfaces;
using IAutoM8.Neo4jRepository.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Neo4j.Driver.V1;

namespace IAutoM8.Neo4jRepository
{
    public static class Module
    {
        public static void Configure(IServiceCollection services)
        {
            services.AddSingleton(ser =>
            {
                var opt = ser.GetService<IOptions<Neo4jOptions>>();
                return GraphDatabase.Driver(opt.Value.Server, AuthTokens.Basic(opt.Value.UserName, opt.Value.Password));
            });
            services.AddScoped<IDataAccess, DataAccess>();
            services.AddScoped<IResourceNeo4jRepository, ResourceNeo4jRepository>();
            services.AddScoped<ITaskNeo4jRepository, TaskNeo4jRepository>();
            services.AddScoped<IFormulaTaskNeo4jRepository, FormulaTaskNeo4jRepository>();
            services.AddScoped<IFormulaNeo4jRepository, FormulaNeo4jRepository>();
        }
    }
}
