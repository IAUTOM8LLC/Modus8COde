using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IAutoM8.Service.Payment.Dto;
using IAutoM8.Service.Payment.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace IAutoM8.Controllers.api
{
    [Produces("application/json")]
    [Route("api/Payment")]
    public class PaymentController : Controller
    {

        private readonly IPaymentService _paymentService;
        private readonly System.Security.Claims.ClaimsPrincipal _principal;
        public PaymentController(IPaymentService paymentService, System.Security.Claims.ClaimsPrincipal principal)
        {
            _paymentService = paymentService;
            _principal = principal;
        }


        [HttpGet]
        [Route("GetFundRequests/{type:int}")]
        public async Task<IActionResult> GetFundRequests(int type)
        {
            var result = await _paymentService.GetPaymentRequests(type);
            return Ok(result);
        }

        [HttpGet]
        [Route("AcceptDenyRequest")]
        public async Task<IActionResult> AcceptDenyRequest([FromQuery]int requestId, [FromQuery]int response,[FromQuery]string desc)
        {
            var result = await _paymentService.AcceptDenyRequest(requestId, response, desc);
            return Ok(result);
        }

        [HttpPost]
        [Route("DownloadCSV")]
        public async Task<IActionResult> DownloadCSV([FromBody]PaymentRequestDto requestDto)
        {
            var result = await _paymentService.DownloadCSV(requestDto);
            return Ok(result);
        }

        [HttpPost]
        [Route("BatchProcess/{isAccepted:int}")]
        public async Task<IActionResult> BatchProcess([FromBody]PaymentRequestDto requestDto, [FromRoute] int isAccepted)
        {
            var result = await _paymentService.BatchProcess(requestDto, isAccepted);
            return Ok(result);
        }
    }
}
