using IAutoM8.InfusionSoft.Dto;
using IAutoM8.InfusionSoft.Extentions;
using IAutoM8.InfusionSoft.Infrastructure;
using IAutoM8.InfusionSoft.Interfaces;
using IAutoM8.InfusionSoft.Responces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Linq;
using System.Threading.Tasks;

namespace IAutoM8.InfusionSoft
{
    class ContactService : BaseService, IContactService
    {
        private readonly IInfusionSoftConfiguration _infusionSoftConfiguration;
        private readonly IMapperService _mapperService;
        private readonly IOptions<InfusionSoftDataOptions> _options;
        public ContactService(IInfusionSoftConfiguration infusionSoftConfiguration,
            IMapperService mapperService,
            ILogger<ContactService> logger,
            IOptions<InfusionSoftDataOptions> options)
            : base(infusionSoftConfiguration, logger)
        {
            _infusionSoftConfiguration = infusionSoftConfiguration;
            _mapperService = mapperService;
            _options = options;
        }

        public async Task<int> AddAsync(ContactDto contactDto)
        {
            var doc = GenerateDoc(
                d => d
                    .Method("ContactService.add")
                    .Params(p => p
                        .Param(_infusionSoftConfiguration.ApiKey)
                        .Param(contactDto)));
            
            var result = await PostAndGetResponceAsync(doc);

            return _mapperService.MapToStruct<int>(result);
        }

        public async Task<bool> ApplyConfirmActionAsync(int contactId)
        {
            var doc = GenerateDoc(
                d => d
                    .Method("ContactService.runActionSequence")
                    .Params(p => p
                        .Param(_infusionSoftConfiguration.ApiKey)
                        .Param(contactId)
                        .Param(_options.Value.OnUserConfirmedActionId)));

            var result = await PostAndGetResponceAsync(doc);

            var actions = _mapperService.MapToList<ActionItemResponce>(result);
            return actions.All(w => !w.IsError);
        }

        public async Task<bool> ApplyRegisterActionAsync(int contactId)
        {
            var doc = GenerateDoc(
                d => d
                    .Method("ContactService.runActionSequence")
                    .Params(p => p
                        .Param(_infusionSoftConfiguration.ApiKey)
                        .Param(contactId)
                        .Param(_options.Value.OnUserRegisteredActionId)));

            var result = await PostAndGetResponceAsync(doc);

            var actions = _mapperService.MapToList<ActionItemResponce>(result);
            return actions.All(w => !w.IsError);
        }
    }
}
