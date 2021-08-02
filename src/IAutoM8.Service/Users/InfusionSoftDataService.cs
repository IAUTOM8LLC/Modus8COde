using IAutoM8.InfusionSoft.Interfaces;
using IAutoM8.Service.Users.Dto;
using IAutoM8.Service.Users.Interfaces;
using System.Threading.Tasks;

namespace IAutoM8.Service.Users
{
    public class InfusionSoftDataService : IInfusionSoftDataService
    {
        private readonly IInfusionSoftConfiguration _infusionSoftConfiguration;

        public InfusionSoftDataService(IInfusionSoftConfiguration infusionSoftConfiguration)
        {
            _infusionSoftConfiguration = infusionSoftConfiguration;
        }

        public async Task<InfusionSoftDataDto> GetDataAsync()
        {
            return await Task.FromResult(new InfusionSoftDataDto
            {
                OrderFormUrl = _infusionSoftConfiguration.OrderFormUri
            });
        }
    }
}
