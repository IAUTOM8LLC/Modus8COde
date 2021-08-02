using IAutoM8.Controllers.Abstract;
using IAutoM8.Service.Resources.Dto;
using IAutoM8.Service.Resources.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace IAutoM8.Controllers.api
{
    [Route("api/resource")]
    public class ResourceController : BaseController
    {
        private readonly IResourceService _resourceService;
        private readonly IResourceHubService _resourceHubService;
        public ResourceController(IResourceService resourceService,
            IResourceHubService resourceHubService)
        {
            _resourceService = resourceService;
            _resourceHubService = resourceHubService;
        }

        [HttpPost]
        [Authorize(Roles = VendorOrBussinessMemberOrAdmin)]
        [Route("upload-file")]
        public async Task<IActionResult> UploadFile(IFormFile qqfile)
        {
            //return Content(await _resourceService.UploadTempFile(qqfile), "text/plain");
            return Content(await _resourceService.UploadTempFile(qqfile), "text/plain") ; 
        }

        [HttpPost]
        //[Authorize(Roles = VendorOrBussinessMemberOrAdmin)]
        [Route("temp-upload-file")]
        public async Task<IActionResult> UploadTempFile(IFormFile qqfile)
        {
            return Content(await _resourceService.UploadTempExcelFile(qqfile), "text/plain");
        }

        [HttpPost]
        [Authorize(Roles = OwnerOrManager)]
        [Route("upload-description-file")]
        public async Task<IActionResult> UploadDescriptionFile(IFormFile file)
        {
            return Ok(await _resourceService.UploadDescriptionFile(file));
        }

        [HttpDelete]
        [Authorize(Roles = BussinessMemeber)]
        [Route("delete-file")]
        public IActionResult DeleteFile()
        {
            return Content(JsonConvert.SerializeObject(new
            {
                success = true
            }));
        }

        [HttpPut]
        [Authorize(Roles = OwnerOrManagerOrAdmin)]
        [Route("formula")]
        public async Task<IActionResult> UpdateFormulaResource([FromBody]UpdateResourceDto resource)
        {
            await _resourceService.UpdateFormulaResources(resource);
            return Ok();
        }

        [HttpPut]
        [Authorize(Roles = OwnerOrManagerOrAdmin)]
        [Route("formula-task")]
        public async Task<IActionResult> UpdateFormulaTaskResource([FromBody]UpdateTaskResourceDto resource)
        {
            await _resourceService.UpdateFormulaTaskResources(resource);
            return Ok();
        }

        [HttpPut]
        [Authorize(Roles = OwnerOrManager)]
        [Route("project")]
        public async Task<IActionResult> UpdateProjectResource([FromBody]UpdateResourceDto resource)
        {
            await _resourceService.UpdateProjectResources(resource);
            return Ok();
        }

        [HttpPut]
        //[Authorize(Roles = VendorOrBussinessMember)]
        //[Authorize(Roles = VendorOrBussinessOrCompanyWorkerMember)]
        [Authorize(Roles = VendorOrBussinessOrCompanyWorkerOrCompanyMember)] // added new combination for company and company worker (WRT Sprint 10B). 
        [Route("project-task")]
        public async Task<IActionResult> UpdateProjectTaskResource([FromBody]UpdateTaskResourceDto resource)
        {
            await _resourceService.UpdateProjectTaskResources(resource);
            return Ok();
        }

        [HttpGet]
        [Authorize]
        [Route("get-project-files/{projectId:int}")]
        public async Task<IActionResult> GetProjectFiles(int projectId)
        {
            return Ok(await _resourceHubService.GetProjectResorcesAsync(projectId));
        }

        [HttpGet]
        [Authorize]
        [Route("get-profile-image")]
        public async Task<IActionResult> GetProfileImage()
        {
            var result = await _resourceService.GetProfileImage(UserGuid);
            return Ok(result);
        }

        [HttpPost]
        [Authorize]
        [Route("upload-profile-image")]
        public async Task<IActionResult> UploadProfileImage(IFormFile qqfile)
        {
            return Content(await _resourceService.UploadProfileImage(qqfile), "text/plain");
        }
    }
}
