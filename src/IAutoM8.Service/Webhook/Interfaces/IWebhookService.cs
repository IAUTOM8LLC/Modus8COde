using IAutoM8.Service.Webhook.Dto;
using System.Threading.Tasks;

namespace IAutoM8.Service.Webhook.Interfaces
{
    public interface IWebhookService
    {
        Task ProccessInvoiceAsync(InvoiceWebhookDto invoiceWebhookDto);
    }
}
