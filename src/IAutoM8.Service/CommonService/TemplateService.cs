using IAutoM8.Service.CommonService.Interfaces;
using RazorLight;
using System.Threading.Tasks;

namespace IAutoM8.Service.CommonService
{
    public class TemplateService: ITemplateService
    {
        private readonly IRazorLightEngine _razorLightEngine;

        public TemplateService(IRazorLightEngine razorLightEngine)
        {
            _razorLightEngine = razorLightEngine;
        }

        public async Task<string> BuildEmailAsync<T>(string viewName, T model)
        {
            return await _razorLightEngine.CompileRenderAsync($"\\Email\\{viewName}.cshtml", model);
        }
    }
}
