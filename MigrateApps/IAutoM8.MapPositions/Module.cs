using IAutoM8.Global.Options;
using Microsoft.Extensions.DependencyInjection;

namespace IAutoM8.MapPositions
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
            Repository.Infrastructure.Module.Configure(services);
            services.AddScoped<IWorker, Worker>();
        }
    }
}
