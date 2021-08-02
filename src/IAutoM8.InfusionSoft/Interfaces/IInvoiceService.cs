using IAutoM8.InfusionSoft.Dto;
using System.Threading.Tasks;

namespace IAutoM8.InfusionSoft.Interfaces
{
    public interface IInvoiceService
    {
        Task<InvoiceDto> GetDetailAsync(int invoiceId);
    }
}
