using IAutoM8.InfusionSoft.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace IAutoM8.InfusionSoft.Infrastructure
{
    public static class Module
    {
        public static void ConfigureInfusionSoftApi(this IServiceCollection services)
        {
            services.AddSingleton<IInfusionSoftConfiguration, InfusionSoftConfiguration>();
            services.AddScoped<IContactService, ContactService>();
            services.AddScoped<IOrderService, OrderService>();
            services.AddScoped<IAffiliateService, AffiliateService>();
            services.AddSingleton<IMapperService, MapperService>();
            services.AddScoped<IInvoiceService, InvoiceService>();
            Mapper.Register();
        }
    }
}
