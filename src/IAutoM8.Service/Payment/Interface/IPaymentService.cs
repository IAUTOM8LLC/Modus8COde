using IAutoM8.Domain.Models.Credits;
using IAutoM8.Service.Payment.Dto;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace IAutoM8.Service.Payment.Interface
{
    public interface IPaymentService
    {
        Task<IList<PaymentDto>> GetPaymentRequests(int type);
        Task<TransferRequest> AcceptDenyRequest(int requestId, int response, string desc);
        Task<List<string>> DownloadCSV(PaymentRequestDto payments);
        Task<List<string>> BatchProcess(PaymentRequestDto payments,int isAccepted);
    }
}
