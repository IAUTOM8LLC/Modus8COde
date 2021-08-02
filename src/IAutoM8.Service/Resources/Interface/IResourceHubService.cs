using System.Collections.Generic;
using System.Threading.Tasks;
using IAutoM8.Service.Resources.Dto;

namespace IAutoM8.Service.Resources.Interface
{
    public interface IResourceHubService
    {
        Task<IEnumerable<ResourceInfoDto>> GetProjectResorcesAsync(int projectId);
    }
}
