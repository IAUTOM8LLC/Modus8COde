
using IAutoM8.Service.Webhook.Dto;
using IAutoM8.Service.Webhook.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace IAutoM8.Controllers
{
    public class WebhookController : Controller
    {
        private readonly IWebhookService _webhookService;
        public WebhookController(IWebhookService webhookService)
        {
            _webhookService = webhookService;
        }
        [HttpPost]
        public async Task<IActionResult> Invoice([FromBody]InvoiceWebhookDto invoiceWebhookDto)
        {
            await _webhookService.ProccessInvoiceAsync(invoiceWebhookDto);
            return Ok();
        }
    }
}
