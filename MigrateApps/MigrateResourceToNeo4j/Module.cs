using IAutoM8.Global.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.WindowsAzure.Storage;

namespace MigrateResourceToNeo4j
{
    public static class Module
    {
        public static void Configure(IServiceCollection services)
        {
            services.AddOptions();
            services.Configure<Neo4jOptions>(opt =>
            {
                opt.Server = "bolt://neo4j-prod-vm.eastus.cloudapp.azure.com:7687";
                opt.UserName = "neo4j";
                opt.Password = "YnS9uU2RaM-w86F@";
            });
            IAutoM8.Neo4jRepository.Module.Configure(services);
            services.AddScoped<IWorker, Worker>();

            services.AddSingleton(CloudStorageAccount.Parse("DefaultEndpointsProtocol=https;AccountName=iautoprod;AccountKey=RmfizbeYMX4BDRLWIILVfizyjl4QxfbTHLT0QmdOPUdp4Dlfdjyp8EAKu/hjILSbO+XFVjoYQnKZj92INAL/Ow==;EndpointSuffix=core.windows.net"));
            services.AddSingleton(s => s.GetService<CloudStorageAccount>().CreateCloudBlobClient());
        }
    }
}
