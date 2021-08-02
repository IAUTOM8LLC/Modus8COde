using IAutoM8.InfusionSoft.Dto;
using IAutoM8.InfusionSoft.Extentions;
using IAutoM8.InfusionSoft.Interfaces;
using IAutoM8.InfusionSoft.Responces;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace IAutoM8.InfusionSoft
{
    class InvoiceService : BaseService, IInvoiceService
    {
        private readonly IInfusionSoftConfiguration _infusionSoftConfiguration;
        private readonly IMapperService _mapperService;
        public InvoiceService(IInfusionSoftConfiguration infusionSoftConfiguration,
            IMapperService mapperService,
            ILogger<InvoiceService> logger)
            : base(infusionSoftConfiguration,
                  logger)
        {
            _infusionSoftConfiguration = infusionSoftConfiguration;
            _mapperService = mapperService;
        }

        public async Task<InvoiceDto> GetDetailAsync(int invoiceId)
        {
            var str = GenerateDoc(
                d => d
                    .Method("DataService.load")
                    .Params(p => p
                        .Param(_infusionSoftConfiguration.ApiKey)
                        .Param("Invoice")
                        .Param(invoiceId)
                        .Param(new string[] {
                        "ContactId",
                        "PayStatus"
                        })));

            var result = await PostAndGetResponceAsync(str);

            return _mapperService.Map<InvoiceDto>(result);
        }
    }
}
