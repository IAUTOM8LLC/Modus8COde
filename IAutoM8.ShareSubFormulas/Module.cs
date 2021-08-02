using IAutoM8.Global.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.WindowsAzure.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Memory;
using System.Collections.Generic;
using System.Security.Claims;
using System;

namespace IAutoM8.ShareSubFormulas
{
    public static class Module
    { 
        public static void Configure(IServiceCollection services)
        {
            services.AddScoped(ser => new ClaimsPrincipal(new ClaimsIdentity(new List<Claim> {
                    new Claim(ClaimTypes.PrimarySid, Guid.Empty.ToString()) })));
            services.AddOptions();
            services.Configure<DbOptions>(opt =>
            {
                opt.ConnectionString = "Server=tcp:iautom8.database.windows.net,1433;Initial Catalog=modus-staging;Persist Security Info=False;User ID=matt.versteeg;Password=IAutoM8ShareHolderDb;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
            });
            services.Configure<Neo4jOptions>(opt =>
            {
                opt.Server = "bolt://neo4j-stage-vm.eastus.cloudapp.azure.com:7687";
                opt.UserName = "neo4j";
                opt.Password = "K\"kn<n]B#C@QKe7z";
            });
            Neo4jRepository.Module.Configure(services);
            Repository.Infrastructure.Module.Configure(services);
            Service.Infrastructure.Module.ConfigureMapper();
            Service.Infrastructure.Module.ConfigureServices(services,
            new ConfigurationRoot(new List<IConfigurationProvider> { new MemoryConfigurationProvider(new MemoryConfigurationSource
            {
                InitialData = new Dictionary<string, string>
                {
                    { "UseHangfire", "false" },
                    { "AzureBlobSetting:ConnectionString", "DefaultEndpointsProtocol=https;AccountName=iautostage;AccountKey=/JybGQOCyXEzEKqN3HS2KWgFOaU4l5h8Kpj5cnmX46dApJsOuzHCUTJAySBIOKEvRX6fzPeg+YvthePfet0Qzg==;EndpointSuffix=core.windows.net"}
                }
            }) }));
            services.AddScoped<IWorker, Worker>();
        }
    }
}
