using System;
using System.Threading.Tasks;
using IAutoM8.Controllers.Abstract;
using IAutoM8.Service.Braintree.Interfaces;
using IAutoM8.Service.ProjectTasks.Dto;
using IAutoM8.Service.Vendor.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IAutoM8.Controllers.api
{
    [Authorize]
    public class CreditsController : BaseController
    {
        private readonly ICreditsService _creditsService;

        public CreditsController(ICreditsService creditsService)
        {
            _creditsService = creditsService;
        }

        [HttpGet]
        [Route("token")]
        public async Task<IActionResult> GetToken()
        {
            var result = await _creditsService.GetToken();
            return Ok(result);
        }

        [Route("buy-credits")]
        [HttpPost]
        public async Task<IActionResult> BuyCredits([FromBody] BraintreeDto braintreeDto)
        {
            var result = await _creditsService.TransferRequest(braintreeDto);
            return Ok(result);
        }

        [Route("add-credits/{amount:decimal}/{userId}")]
        [HttpGet]
        public async Task<IActionResult> AddCredits(decimal amount, Guid userId)
        {
            await _creditsService.AddCredits(amount, userId);
            return Ok();
        }



        [HttpGet]
        [Route("load-credits")]
        public async Task<IActionResult> LoadCredits()
        {
            var result = await _creditsService.GetOwnerCredits();

            return Ok(result);
        }

        [HttpGet]
        [Route("load-balance")]
        public async Task<IActionResult> LoadBalance()
        {
            var result = await _creditsService.GetBalance();

            return Ok(result);
        }

        [HttpGet]
        [Route("transfer-request")]
        public async Task<IActionResult> TransferRequest()
        {
            var result = await _creditsService.RequestTransfer();
            return Ok(result);
        }

        [HttpGet]
        [Route("vendor-tax")]
        public async Task<IActionResult> GetVendorTaxt()
        {
            var result = await _creditsService.GetVendorTax();

            return Ok(result);
        }

        [HttpGet]
        [Route("load-active-transfer")]
        public async Task<IActionResult> LoadActiveTransferRequest()
        {
            var result = await _creditsService.LoadActiveTransferRequest();

            return Ok(result);
        }

        [HttpGet]
        [Route("accept-transfer-request/{transferRequestId:int}")]
        public async Task<IActionResult> AcceptTransferRequest(int transferRequestId)
        {
            await _creditsService.AcceptTransferRequest(transferRequestId);
            return Ok();
        }

        [HttpGet]
        [Route("add-fund-request/{amount:int}")]
        public async Task<IActionResult> AddVendorFundRequest(decimal amount)
        {
            var result = await _creditsService.AddVendorFundRequest(amount);
            return Ok(result);
        }
    }
}
