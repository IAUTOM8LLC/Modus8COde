using Hangfire;
using IAutoM8.Domain.Models.User;
using IAutoM8.Global.Options;
using IAutoM8.Infrastructure.Filters;
using IAutoM8.Infrastructure.JWT;
using IAutoM8.InfusionSoft.Infrastructure;
using IAutoM8.Repository;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NETCore.MailKit.Extensions;
using NETCore.MailKit.Infrastructure.Internal;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace IAutoM8.Infrastructure.Extensions
{
    public static class ServiceCollectionExtensions
    {
        private const string EmailConfirmationTokenProviderName = "ConfirmEmail";

        public static IServiceCollection AddCompression(this IServiceCollection services)
        {
            return services.AddResponseCompression(options =>
            {
                options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[]
                {
                    "image/svg+xml",
                    "application/font-woff2"
                });
            });
        }

        public static IServiceCollection ConfigureOptions(this IServiceCollection services)
        {
            services.AddOptions();

            services.Configure<DbOptions>(opt =>
            {
                opt.ConnectionString = Startup.Configuration.GetConnectionString("IAutoM8");
            });

            services.Configure<JwtOptions>(opt =>
            {
                var jwt = Startup.Configuration.GetSection("JWT");
                opt.JwtSecret = jwt["SECRET"];
                opt.JwtIssuer = jwt["ISSUER"];
                opt.JwtAudience = jwt["AUDIENCE"];
                opt.JwtExpiration = TimeSpan.FromMinutes(int.Parse(jwt["EXPIRATION"]));
                opt.JwtRememberExpiration = TimeSpan.FromMinutes(int.Parse(jwt["REMEMBER_EXPIRATION"]));
            });

            services.Configure<EmailTemplates>(opt =>
            {
                Startup.Configuration.GetSection("EmailTemplates").Bind(opt);
            });

            services.Configure<BraintreeSettings>(opt =>
            {
                Startup.Configuration.GetSection("BraintreeSettings").Bind(opt);
            });

            services.Configure<Neo4jOptions>(opt =>
                Startup.Configuration.GetSection("Neo4j").Bind(opt));

            services.Configure<AppOptions>(opt =>
            {
                opt.DefaultPassword = Startup.Configuration[nameof(AppOptions.DefaultPassword)];
                opt.AllowUsenameCharacters = Startup.Configuration[nameof(AppOptions.AllowUsenameCharacters)];
            });
            
            services.Configure<AccountConfirmationSetting>(opt =>
            {
                var siteUrl = Startup.Configuration["siteUrl"];
                var section = Startup.Configuration.GetSection("AccountConfirmationSetting");
                opt.SiteUrl = siteUrl;
                opt.ConfirmUrl = siteUrl + section["ConfirmUrl"];
                opt.DefaultPassword = section["DefaultPassword"];
                opt.ForgotPasswordUrl = siteUrl + section["ForgotPasswordUrl"];
                opt.ConfirmOwnerUrl = siteUrl + section["ConfirmOwnerUrl"];
            });
            services.Configure<PredefinedFormulaSeller>(opt =>
            {
                opt.Ids = Startup.Configuration["PredefinedFormulaSeller"].Split(",").Select(s => new Guid(s)).ToArray();
            });
            services.Configure<OutsourceConfirmationSetting>(opt =>
            {
                var siteUrl = Startup.Configuration["siteUrl"];
                var section = Startup.Configuration.GetSection("OutsourceConfirmationSetting");
                opt.Project = siteUrl + section["Project"];
                opt.Formula = siteUrl + section["Formula"];
                opt.AssignWorker = siteUrl + section["AssignWorker"];
                opt.StartWork = siteUrl + section["StartWork"];
                opt.TaskComment = siteUrl + section["TaskComment"];
                opt.TaskInfo = siteUrl + section["TaskInfo"];
                opt.ProjectPage = siteUrl + section["ProjectPage"];
            });
            services.Configure<TransferRequestSettings>(opt =>
            {
                var siteUrl = Startup.Configuration["siteUrl"];
                var section = Startup.Configuration.GetSection("TransferRequestSettings");
                opt.AcceptTransferRequest = siteUrl + section["AcceptTransferRequest"];
            });
            services.Configure<ConfirmEmailDataProtectionTokenProviderOptions>(opt=>{
                opt.Name = "ConfirmEmailDataProtectionTokenProvider";
                opt.TokenLifespan = TimeSpan.FromDays(int.Parse(Startup.Configuration.GetSection("Token")["EmailConfirmationExpiration"]));
            });
            return services;
        }

        public static IServiceCollection ConfigureJwtAuthentication(this IServiceCollection services)
        {
            services
                .AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    var jwt = Startup.Configuration.GetSection("JWT");
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        // The signing key must match!
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = TokenProvider.SigningKey(jwt["SECRET"]),

                        // Validate the JWT Issuer (iss) claim
                        ValidateIssuer = true,
                        ValidIssuer = jwt["ISSUER"],

                        // Validate the JWT Audience (aud) claim
                        ValidateAudience = true,
                        ValidAudience = jwt["AUDIENCE"],

                        // Validate the token expiry
                        ValidateLifetime = true,

                        // If you want to allow a certain amount of clock drift, set that here:
                        ClockSkew = TimeSpan.FromMinutes(1)
                    };
                })
                .AddCookie(options =>
                {
                    options.LoginPath = "/login";

                    options.Events.OnRedirectToAccessDenied = ctx =>
                    {
                        if (ctx.Request.Path.StartsWithSegments("/api") && ctx.Response.StatusCode == 200)
                        {
                            ctx.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                        }

                        return Task.CompletedTask;
                    };

                    options.Events.OnRedirectToLogin = ctx =>
                    {
                        if (ctx.Request.Path.StartsWithSegments("/api")
                            && ctx.Response.StatusCode == (int)HttpStatusCode.OK)
                        {
                            ctx.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                        }
                        else if (!ctx.Request.Path.StartsWithSegments("/api")
                                 && ctx.Response.StatusCode != (int)HttpStatusCode.Unauthorized)
                        {
                            ctx.Response.Redirect(ctx.RedirectUri);
                        }

                        return Task.CompletedTask;
                    };
                });

            return services;
        }

        public static IdentityBuilder ConfigureIdentity(this IServiceCollection services)
        {
            var indetityBuilder = services.AddIdentityCore<User>(options =>
            {
                options.SignIn.RequireConfirmedEmail = true;

                // Password settings
                options.Password = Startup.Configuration.GetSection("Password").Get<PasswordOptions>();

                //Username config
                options.User.RequireUniqueEmail = true;
                options.User.AllowedUserNameCharacters =
                    Startup.Configuration.GetSection("Identity")[nameof(AppOptions.AllowUsenameCharacters)];

                options.Tokens.EmailConfirmationTokenProvider = EmailConfirmationTokenProviderName;
            });

            // Hack for asp core identity 2.0
            // https://github.com/aspnet/Identity/issues/1376
            indetityBuilder = new IdentityBuilder(indetityBuilder.UserType, typeof(Role), indetityBuilder.Services);

            indetityBuilder
                .AddEntityFrameworkStores<Context>()
                .AddDefaultTokenProviders()
                .AddTokenProvider<ConfirmEmailDataProtectorTokenProvider<User>>(EmailConfirmationTokenProviderName);

            return indetityBuilder;
        }

        public static IServiceCollection AddMailKit(this IServiceCollection services)
        {
            services.AddMailKit(optionBuilder =>
            {
                optionBuilder.UseMailKit(Startup.Configuration.GetSection("SMTPSetting").Get<MailKitOptions>());
            });

            services.Configure<SendGridOptions>(opt =>
            {
                var sendGridOptions = Startup.Configuration.GetSection("SMTPSetting");
                opt.Server = sendGridOptions["Server"];
                opt.SenderEmail = sendGridOptions["SenderEmail"];
                opt.Account = sendGridOptions["Account"];
                opt.Password = sendGridOptions["Password"];
                opt.ApiKey = sendGridOptions["ApiKey"];
            });

            return services;
        }

        public static IServiceCollection AddTransferRequest(this IServiceCollection services)
        {
            services.Configure<TransferRequestRecipientsOptions>(opt =>
            {
                Startup.Configuration.GetSection("TransferRequestRecipients").Bind(opt);
            });

            return services;
        }

        public static IServiceCollection AddCustomizedMvc(this IServiceCollection services)
        {
            services.AddMvc(options =>
            {
                // TODO: options.Filters.Add(typeof(ModelValidationFilter));
                options.Filters.Add(typeof(GlobalExceptionFilterAttribute));
            })
            .AddJsonOptions(options =>
            {
                options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            });

            return services;
        }

        public static IServiceCollection RegisterCustomServices(this IServiceCollection services)
        {
            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddTransient<ITokenProvider, TokenProvider>();

            // add this manually because we're using .AddIdentityCore(), not .AddIdentity()
            services.AddScoped<IRoleValidator<Role>, RoleValidator<Role>>();
            services.TryAddScoped<SignInManager<User>, SignInManager<User>>();
            services.AddScoped<RoleManager<Role>, RoleManager<Role>>();

            // Allow to require UserIdentity in all services
            services.AddScoped(provider => provider.GetService<IHttpContextAccessor>().HttpContext?.User);

            Service.Infrastructure.Module.ConfigureMapper();

            // Custom app services
            Service.Infrastructure.Module.ConfigureServices(services, Startup.Configuration);

            return services;
        }
        public static IServiceCollection ConfigureHangfire(this IServiceCollection services)
        {
            services.AddHangfire(config =>
            {
                config.UseSqlServerStorage(Startup.Configuration.GetConnectionString("IAutoM8"));
            });

            return services;
        }

        public static IServiceCollection ConfigureInfusionSoft(this IServiceCollection services)
        {
            
            services.Configure<InfusionSoftOption>(opt =>
                Startup.Configuration.GetSection("InfusionSoftOption").Bind(opt));

            services.Configure<InfusionSoftDataOptions>(opt =>
                Startup.Configuration.GetSection("InfusionSoftData").Bind(opt));

            services.ConfigureInfusionSoftApi();
            return services;
        }
    }
}
