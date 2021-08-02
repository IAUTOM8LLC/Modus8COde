using IAutoM8.Infrastructure.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Swagger;

namespace IAutoM8
{
    public class Startup
    {
        public static IConfigurationRoot Configuration { get; set; }

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables();

            if (env.IsDevelopment())
                builder.AddUserSecrets<Startup>();

            Configuration = builder.Build();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.ConfigureIdentity();

            services
                .ConfigureOptions()
                .ConfigureJwtAuthentication()
                .ConfigureHangfire()
                .ConfigureInfusionSoft()
                .AddMailKit()
                .AddTransferRequest()
                .AddCompression()
                .AddCustomizedMvc()
                .AddSwaggerGen(c =>
                {
                    c.SwaggerDoc("v1", new Info
                    {
                        Title = "IAutoM8 API",
                        Version = "v1"
                    });
                    // UseFullTypeNameInSchemaIds replacement for .NET Core
                    c.CustomSchemaIds(i => i.FullName);
                })
                .RegisterCustomServices();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            Service.Infrastructure.Module.Configure(app);

            if (env.IsProduction())
                app.UseResponseCompression();

            app
                .UseStaticFiles()
                .AddDevMiddlewares()
                .UseAuthentication()
                .UseHangfire()
                .UseMvc(routes =>
                {
                    routes.MapRoute(
                        name: "default",
                        template: "{controller=Home}/{action=Index}/{id?}");

                    routes.MapSpaFallbackRoute(
                        name: "spa-fallback",
                        defaults: new { controller = "Home", action = "Index" });
                });

            Service.Infrastructure.Module.MigrateDatabase(app, loggerFactory.CreateLogger("Startup"));

            Service.Infrastructure.Module.StartReAssignVendorService(app, loggerFactory.CreateLogger("Startup"));
        }
    }
}
