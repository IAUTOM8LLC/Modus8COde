using IAutoM8.InfusionSoft.Extentions;
using IAutoM8.InfusionSoft.Interfaces;
using IAutoM8.InfusionSoft.Responces;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace IAutoM8.InfusionSoft
{
    class OrderService : BaseService, IOrderService
    {
        private readonly IInfusionSoftConfiguration _infusionSoftConfiguration;
        private readonly IMapperService _mapperService;
        public OrderService(IInfusionSoftConfiguration infusionSoftConfiguration,
            IMapperService mapperService,
            ILogger<OrderService> logger)
            : base(infusionSoftConfiguration, logger)
        {
            _infusionSoftConfiguration = infusionSoftConfiguration;
            _mapperService = mapperService;
        }

        public async Task<int> PlaceOrderAsync(int contactId, int creditCardId, int payPlanId, int[] productIds, int[] subscriptionPlanIds, bool processSpecials, string[] promoCodes)
        {
            var doc = GenerateDoc(
                d => d.Method("OrderService.placeOrder")
                    .Params(p => p
                        .Param(_infusionSoftConfiguration.ApiKey)
                        .Param(contactId)
                        .Param(creditCardId)
                        .Param(payPlanId)
                        .Param(productIds)
                        .Param(subscriptionPlanIds)
                        .Param(processSpecials)
                        .Param(promoCodes)));

            var result = await PostAndGetResponceAsync(doc);

            return _mapperService.Map<OrderDetailResponce>(result).OrderId;
        }
    }
}
