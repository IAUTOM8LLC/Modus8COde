using AutoMapper;
using IAutoM8.Domain.Models.User;
using IAutoM8.Global.Options;
using IAutoM8.Repository;
using IAutoM8.Service.Braintree;
using IAutoM8.Service.Braintree.Interfaces;
using IAutoM8.Service.Client;
using IAutoM8.Service.Client.Interfaces;
using IAutoM8.Service.CommonService;
using IAutoM8.Service.CommonService.Interfaces;
using IAutoM8.Service.Company;
using IAutoM8.Service.Company.Interfaces;
using IAutoM8.Service.Formula;
using IAutoM8.Service.Formula.Interfaces;
using IAutoM8.Service.FormulaTasks;
using IAutoM8.Service.FormulaTasks.Interfaces;
using IAutoM8.Service.Hangfire;
using IAutoM8.Service.Hangfire.Interfaces;
using IAutoM8.Service.Notification;
using IAutoM8.Service.Notification.Interfaces;
using IAutoM8.Service.Payment;
using IAutoM8.Service.Payment.Interface;
using IAutoM8.Service.Projects;
using IAutoM8.Service.Projects.Interfaces;
using IAutoM8.Service.ProjectTasks;
using IAutoM8.Service.ProjectTasks.Interfaces;
using IAutoM8.Service.Resources;
using IAutoM8.Service.Resources.Interface;
using IAutoM8.Service.Scheduler;
using IAutoM8.Service.Scheduler.Interfaces;
using IAutoM8.Service.Scheduler.Stub;
using IAutoM8.Service.SendGrid;
using IAutoM8.Service.SendGrid.Interfaces;
using IAutoM8.Service.Skills;
using IAutoM8.Service.Skills.Interfaces;
using IAutoM8.Service.Teams;
using IAutoM8.Service.Teams.Interfaces;
using IAutoM8.Service.Users;
using IAutoM8.Service.Users.Interfaces;
using IAutoM8.Service.Vendor;
using IAutoM8.Service.Vendor.Interfaces;
using IAutoM8.Service.Webhook;
using IAutoM8.Service.Webhook.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage;
using RazorLight;
using SendGrid;
using System;
using System.IO;
using System.Reflection;

namespace IAutoM8.Service.Infrastructure
{
    public static class Module
    {
        public static void Configure(IApplicationBuilder app)
        {
            WebSockets.Infrastructure.Module.Configure(app);
        }

        public static void ConfigureServices(IServiceCollection services, IConfigurationRoot configurationRoot)
        {
            var useHangfire = bool.Parse(configurationRoot["UseHangfire"]);
            var azureBlobConnectionString = configurationRoot.GetSection("AzureBlobSetting")["ConnectionString"];

            Neo4jRepository.Module.Configure(services);
            Repository.Infrastructure.Module.Configure(services);
            WebSockets.Infrastructure.Module.ConfigureServices(services);
            services.AddSingleton(Mapper.Instance);
            services.AddSingleton<IRazorLightEngine>(f => new EngineFactory().ForFileSystem($"{Directory.GetCurrentDirectory()}\\Templates"));
            services.AddSingleton(CloudStorageAccount.Parse(azureBlobConnectionString));
            services.AddSingleton(s => s.GetService<CloudStorageAccount>().CreateCloudBlobClient());
            services.AddScoped<ISendGridClient>(t => new SendGridClient(t.GetService<IOptions<SendGridOptions>>().Value.ApiKey));

            services.AddScoped<ITaskService, TaskService>();
            services.AddScoped<ITaskHistoryService, TaskHistoryService>();
            services.AddScoped<ITaskCommentService, TaskCommentService>();
            services.AddScoped<ITaskScheduleService, TaskScheduleService>();
            services.AddScoped<IFormulaTaskJobService, FormulaTaskJobService>();

            services.AddScoped<IFormulaService, FormulaService>();
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<IFormulaTaskService, FormulaTaskService>();
            services.AddScoped<IFormulaShareService, FormulaShareService>();
            services.AddScoped<IFormulaToProjectConverterService, FormulaToProjectConverterService>();

            services.AddScoped<IDateTimeService, DateTimeService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IUserClaimsPrincipalFactory<User>, UserClaimsPrincipalFactory>();
            services.AddScoped<IProjectService, ProjectService>();
            services.AddScoped<IUserRolePermissionsService, UserRolePermissionsService>();
            services.AddScoped<ITaskStartDateHelperService, TaskStartDateHelperService>();
            services.AddScoped<Func<ITaskStartDateHelperService>>(ser => ser.GetService<ITaskStartDateHelperService>);

            services.AddScoped<ISkillService, SkillService>();
            services.AddScoped<IClientService, ClientService>();
            services.AddScoped<ICreditsService, CreditsService.CreditsService>();
            services.AddScoped<INotificationService, NotificationService>();
            services.AddScoped<ISendGridService, SendGridService>();
            services.AddScoped<IStorageService, StorageService>();
            services.AddScoped<IEntityFrameworkPlus, EntityFrameworkPlus>();
            services.AddScoped<IResourceService, ResourceService>();
            services.AddScoped<INotificationSettingsService, NotificationSettingsService>();
            services.AddScoped<ITemplateService, TemplateService>();
            services.AddScoped<ITaskImportService, TaskImportService>();
            services.AddScoped<ITaskFormulaService, TaskFormulaService>();
            services.AddScoped<IProjectTaskEntityImportService, ProjectTaskEntityImportService>();
            services.AddScoped<IResourceHubService, ResourceHubService>();
            services.AddScoped<IHangfireService, HangfireService>();
            services.AddScoped<IFormulaTaskOutsourcesService, FormulaTaskOutsourcesService>();
            services.AddScoped<IProjectTaskOutsourcesService, TaskOutsourcesService>();
            services.AddScoped<IVendorService, VendorService>();
            services.AddScoped<ICompanyService, CompanyService>();
            services.AddScoped<INotificationManagingService, NotificationManagingService>();

            services.AddScoped<IInfusionSoftDataService, InfusionSoftDataService>();
            services.AddScoped<IWebhookService, WebhookService>();

            services.AddScoped<ITeamService, TeamService>();
            services.AddScoped<IPaymentService, PaymentService>();



            if (useHangfire)
            {
                services.AddTransient<IScheduleService, ScheduleService>();
                services.AddTransient<IJobService, JobService>();
                services.AddTransient<JobService>();
                services.AddTransient<Func<IScheduleService>>(ser => ser.GetService<IScheduleService>);
                services.AddTransient<Func<ITaskService>>(ser => ser.GetService<ITaskService>);
            }
            else
            {
                services.AddTransient<IScheduleService, StubScheduleService>();
                services.AddTransient<IJobService, StubJobService>();
                services.AddTransient<StubJobService>();
                services.AddTransient<Func<IScheduleService>>(ser => ser.GetService<IScheduleService>);
            }
        }

        public static void ConfigureMapper()
        {
            Mapper.Initialize(cfg => { cfg.AddProfiles(Assembly.GetExecutingAssembly()); });

#if DEBUG
            Mapper.AssertConfigurationIsValid();
#endif
        }

        public static void MigrateDatabase(IApplicationBuilder app, ILogger log)
        {
            try
            {
                using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>()
                    .CreateScope())
                {
                    var context = serviceScope.ServiceProvider.GetService<Context>();
                    context.Database.Migrate();

                    log.LogInformation("Successfully migrated database");
                }
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Failed to migrate  database");
            }
        }

        public static void StartReAssignVendorService(IApplicationBuilder app, ILogger logger)
        {
            try
            {
                using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
                {
                    var scheduleService = serviceScope.ServiceProvider.GetService<IScheduleService>();
                    scheduleService.ReAssignVendors();

                    logger.LogInformation("Successfully started ReAssignVendors Service");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to start ReAssignVendors Service");
            }
        }
    }
}
