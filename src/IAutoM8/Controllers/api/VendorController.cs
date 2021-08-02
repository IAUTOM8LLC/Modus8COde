using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IAutoM8.Controllers.Abstract;
using IAutoM8.Domain.Models.User;
using IAutoM8.Service.ProjectTasks.Dto;
using IAutoM8.Service.ProjectTasks.Interfaces;
using IAutoM8.Service.Vendor.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IAutoM8.Controllers.api
{
    [Authorize]
    public class VendorController : BaseController
    {
        private readonly IVendorService _vendorService;

        public VendorController(IVendorService vendorService)
        {
            _vendorService = vendorService;
        }

        [HttpGet]
        [Route("projectTask/{notificationId:int}")]
        public async Task<IActionResult> GetProjectTaskNotification(int notificationId)
        {
            var result = await _vendorService.GetProjectTaskVendorNotification(notificationId);
            return Ok(result);
        }

        [HttpGet]
        [Route("formulaTask/{notificationId:int}")]
        public async Task<IActionResult> GetFormulaTaskNotification(int notificationId)
        {
            var result = await _vendorService.GetFormulaTaskVendorNotification(notificationId);
            return Ok(result);
        }

        [HttpGet]
        [Route("selected-vendors/{formulaId:int}")]
        public async Task<IActionResult> GetSelectedVendorsByFormulaId(int formulaId)
        {
            var result = await _vendorService.GetSelectedVendorsByFormulaId(formulaId);
            return Ok(result);
        }

        [HttpGet]
        [Route("selected-vendors/{formulaId:int}/{formulaTaskId:int}/{optionType}")]
        public async Task<IActionResult> GetSelectedVendorsByTaskId(int formulaId, int formulaTaskId, string optionType)
        {
            var result = await _vendorService.GetSelectedVendorsByTaskId(formulaId, formulaTaskId, optionType);
            return Ok(result);
        }

        [HttpPost]
        [Route("formulaTaskResponse")]
        public async Task<IActionResult> FormulaTaskNotificationResponse([FromBody] FormulaTaskVendorDto formulaTaskVendorDto)
        {
            await _vendorService.UpdateFormulaTaskVendorNotificationStatus(formulaTaskVendorDto);
            return Ok();
        }

        [HttpPost]
        [Route("projectTaskResponse")]
        public async Task<IActionResult> ProjectTaskNotificationResponse([FromBody]ProjectTaskVendorDto projectTaskVendorDto)
        {
            await _vendorService.UpdateProjectTaskVendorNotificationStatus(projectTaskVendorDto);
            return Ok();
        }

        [HttpPut]
        [Route("modify-price/{formulaTaskId:int}/{price:decimal}")]
        public async Task<IActionResult> ModifyFormulaTaskPrice(int formulaTaskId, decimal price)
        {
            await _vendorService.UpdatePriceForFormulaTask(UserGuid, formulaTaskId, price);
            return Ok();
        }

        [HttpDelete]
        [Route("delete-price/{formulaTaskId:int}")]
        public async Task<IActionResult> DeleteFormulaTaskPrice(int formulaTaskId)
        {
            await _vendorService.DeletePriceForFormulaTask(UserGuid, formulaTaskId);
            return Ok();
        }

        [HttpGet]
        [Route("performance-data")]
        public async Task<IActionResult> GetPerformanceData()
        {
            var result = await _vendorService.GetPerformanceData(UserGuid);
            return Ok(result);
        }

        //[HttpGet]
        //[Route("company-performance-data")]
        //public async Task<IActionResult> GetPerformanceDataForCompany()
        //{
        //    var result = await _vendorService.GetPerformanceDataForCompany(UserGuid);
        //    return Ok(result);
        //}

        //[HttpGet]
        //[Route("company-user-details")]
        //public async Task<IActionResult> GetCompanyUserDetails()
        //{
        //    var result = await _vendorService.GetCompanyUserDetails(UserGuid);
        //    return Ok(result);
        //}

        [HttpGet]
        [Route("upskills")]
        public async Task<IActionResult> GetVendorUpSkills()
        {
            var result = await _vendorService.GetVendorUpSkills(UserGuid);
            return Ok(result);
        }

        [HttpGet]
        [Route("certify/formulatask/{formulaTaskId:int}")]
        public async Task<IActionResult> GetFormulaTaskForCertification(int formulaTaskId)
        {
            var result = await _vendorService.GetFormulaTaskCertification(UserGuid, formulaTaskId);
            return Ok(result);
        }

        [HttpPost]
        [Route("certificationResponse")]
        public async Task<IActionResult> FormulaTaskCertificationResponse([FromBody] FormulaTaskVendorDto formulaTaskVendorDto)
        {
            await _vendorService.UpdateFormulaTaskVendorCertificationStatus(formulaTaskVendorDto, UserGuid);
            return Ok();
        }

        [HttpGet]
        [Route("snapshotdetail")]
        public async Task<IActionResult> GetSnapshotDetail()
        {
            SnapshotDetailDto result = await _vendorService.GetSnapshotDetail(UserGuid);
            return Ok(result);

        }

        [HttpPost]
        [Route("syncvendordata")]
        public async Task<IActionResult> SyncVendorData()
        {
            await _vendorService.SyncVendorData(UserGuid);
            return Ok();
        }

        [HttpGet]
        [Route("sendetanotification/{taskId:int}")]
        public async Task<IActionResult> SendETANotification(int taskId)
        {
            await _vendorService.SendETANotification(taskId);
            return Ok();
        }
        [HttpGet]
        [Route("formula-bids")]
        public async Task<IActionResult> GetFormulaBids()
        {
            var result = await _vendorService.GetVendorsTaskBids(UserGuid);
            return Ok(result);
        }
    }
}
