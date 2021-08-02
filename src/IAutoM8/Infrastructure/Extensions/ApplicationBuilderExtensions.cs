using Hangfire;
using IAutoM8.Infrastructure.Hangfire;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SpaServices.Webpack;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using NLog.Web;

namespace IAutoM8.Infrastructure.Extensions
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseCustomSwaggerApi(this IApplicationBuilder app)
        {
            // Enable middleware to serve generated Swagger as a JSON endpoint
            app.UseSwagger();
            // Enable middleware to serve swagger-ui assets (HTML, JS, CSS etc.)
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "V1 Docs");
            });

            return app;
        }

        public static IApplicationBuilder AddDevMiddlewares(this IApplicationBuilder app)
        {
            var env = app.ApplicationServices.GetRequiredService<IHostingEnvironment>();
            var loggerFactory = app.ApplicationServices.GetRequiredService<ILoggerFactory>();

            if (env.IsDevelopment())
            {
                loggerFactory.AddConsole(Startup.Configuration.GetSection("Logging"));

                app.UseDeveloperExceptionPage();

                app.UseWebpackDevMiddleware(new WebpackDevMiddlewareOptions
                {
                    HotModuleReplacement = true,
                    ReactHotModuleReplacement = true
                });

                app.UseCustomSwaggerApi();
            }

            env.ConfigureNLog("nlog.config");
            loggerFactory.AddNLog();
            app.AddNLogWeb();

            return app;
        }

        public static IApplicationBuilder UseHangfire(this IApplicationBuilder app)
        {
            if (bool.TryParse(Startup.Configuration["UseHangfire"], out var useHangfire) && useHangfire)
            {
                GlobalConfiguration.Configuration.UseActivator(new HangfireActivator(app.ApplicationServices));

                app.UseHangfireServer();
                app.UseHangfireDashboard(options: new DashboardOptions
                {
                    AppPath = Startup.Configuration["siteUrl"],
                    Authorization = new[] {new MyAuthorizationFilter()}
                });
            }

            return app;
        }
    }
}
