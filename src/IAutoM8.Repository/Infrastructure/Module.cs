using IAutoM8.Global.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace IAutoM8.Repository.Infrastructure
{
    public static class Module
    {
        public static void Configure(IServiceCollection services)
        {
            services.AddTransient<IRepo, Repo>();
            services.AddTransient(provider => new Context(provider.GetService<IOptions<DbOptions>>()));
        }
    }
}
