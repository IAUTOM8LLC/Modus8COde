using IAutoM8.Domain.Models.User;
using IAutoM8.InfusionSoft.Interfaces;
using IAutoM8.Repository;
using IAutoM8.Service.Webhook.Dto;
using IAutoM8.Service.Webhook.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace IAutoM8.Service.Webhook
{
    public class WebhookService : IWebhookService
    {
        private readonly IRepo _repo;
        private readonly IInvoiceService _invoiceService;
        private readonly ILogger _logger;
        public WebhookService(
             IRepo repo,
             IInvoiceService invoiceService,
             ILogger<WebhookService> logger)
        {
            _repo = repo;
            _invoiceService = invoiceService;
            _logger = logger;
        }
        public async Task ProccessInvoiceAsync(InvoiceWebhookDto invoiceWebhookDto)
        {
            _logger.LogInformation(JsonConvert.SerializeObject(invoiceWebhookDto));
            using (var trx = _repo.Transaction())
            {
                foreach(var invoice in invoiceWebhookDto.ObjectKeys)
                {
                    var invoiceDetail = await _invoiceService.GetDetailAsync(invoice.Id);
                    var profile = await trx.Track<UserProfile>()
                        .Include(i => i.User)
                        .FirstOrDefaultAsync(w => w.ContactId == invoiceDetail.ContactId);
                    if (profile == null)
                    {
                        _logger.LogWarning($"Don't find profile with contactId {invoiceDetail.ContactId}");
                    }
                    else
                    {
                        profile.User.IsPayed = invoiceDetail.IsPayed;
                    }
                }
                await trx.SaveChangesAsync();
            }
        }
    }
}
