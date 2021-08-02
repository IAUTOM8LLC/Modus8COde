using IAutoM8.Service.CommonDto;
using IAutoM8.Service.Resources.Dto;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace IAutoM8.Service.Resources.Interface
{
    public interface IResourceService
    {
        Task<string> GetProfileImage(Guid userId);
        Task<string> UploadTempFile(IFormFile file);
        Task<string> UploadTempExcelFile(IFormFile file);

        Task<string> UploadProfileImage(IFormFile file);
        Task<string> UploadDescriptionFile(IFormFile file);
        Task UpdateFormulaResources(UpdateResourceDto resource);
        Task UpdateFormulaTaskResources(UpdateTaskResourceDto resource);
        Task UpdateProjectResources(UpdateResourceDto resource);
        Task UpdateProjectTaskResources(UpdateTaskResourceDto resource);
    }
}
