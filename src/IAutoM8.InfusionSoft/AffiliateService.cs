using IAutoM8.InfusionSoft.Dto;
using IAutoM8.InfusionSoft.Extentions;
using IAutoM8.InfusionSoft.Infrastructure;
using IAutoM8.InfusionSoft.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;

namespace IAutoM8.InfusionSoft
{
    class AffiliateService : BaseService, IAffiliateService
    {
        private readonly IInfusionSoftConfiguration _infusionSoftConfiguration;
        private readonly IMapperService _mapperService;
        public AffiliateService(IInfusionSoftConfiguration infusionSoftConfiguration,
            IMapperService mapperService,
            ILogger<AffiliateService> logger)
            : base(infusionSoftConfiguration, logger)
        {
            _infusionSoftConfiguration = infusionSoftConfiguration;
            _mapperService = mapperService;
        }

        public async Task<int> CreateReferralAsync(AffiliateDto affiliateDto)
        {
            var str = GenerateDoc(
                d => d
                    .Method("DataService.add")
                    .Params(p => p
                        .Param(_infusionSoftConfiguration.ApiKey)
                        .Param("Affiliate")
                        .Param(affiliateDto)));

            var result = await PostAndGetResponceAsync(str);

            return _mapperService.MapToStruct<int>(result);
        }

        public async Task<AffiliateDto> GetReferralAsync(int contactId)
        {
            var str = GenerateDoc(
                d => d
                    .Method("DataService.findByField")
                    .Params(p => p
                        .Param(_infusionSoftConfiguration.ApiKey)
                        .Param("Affiliate")
                        .Param(1)
                        .Param(0)
                        .Param("ContactId")
                        .Param($"{contactId}")
                        .Param(new string[]
                        {
                            "AffCode",
                            "Id",
                            "Password"
                        })));

            var result = await PostAndGetResponceAsync(str);

            return _mapperService.Map<AffiliateDto>(result);
        }
    }
}
