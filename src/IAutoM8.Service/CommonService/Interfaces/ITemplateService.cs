using System.Threading.Tasks;

namespace IAutoM8.Service.CommonService.Interfaces
{
    public interface ITemplateService
    {
        Task<string> BuildEmailAsync<T>(string viewName, T model);
    }
}
