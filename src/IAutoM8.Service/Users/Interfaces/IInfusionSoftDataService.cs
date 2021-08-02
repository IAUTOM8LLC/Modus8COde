using IAutoM8.Service.Users.Dto;
using System.Threading.Tasks;

namespace IAutoM8.Service.Users.Interfaces
{
    public interface IInfusionSoftDataService
    {
        Task<InfusionSoftDataDto> GetDataAsync();
    }
}
