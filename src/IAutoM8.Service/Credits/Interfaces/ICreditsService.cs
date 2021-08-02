using System;
using System.Threading.Tasks;
using IAutoM8.Service.Credits.Dto;
using IAutoM8.Service.CreditsService.Dto;
using IAutoM8.Service.ProjectTasks.Dto;
using IAutoM8.Service.Users.Dto;
using IAutoM8.Service.Vendor.Dto;

namespace IAutoM8.Service.Braintree.Interfaces
{
    public interface ICreditsService
    {
        Task<string> GetToken();
        Task<CreditsDto> TransferRequest(BraintreeDto braintreeDto);
        Task<CreditsDto> GetOwnerCredits();
        Task<BalanceDto> GetBalance();
        Task<CreditsTaxDto> GetVendorTax();
        Task<TransferRequestDto> RequestTransfer();
        Task<TransferRequestDto> LoadActiveTransferRequest();
        Task AcceptTransferRequest(int transferRequestId);
        Task<VendorPaymentRequestDto> AddVendorFundRequest(decimal amount);
        Task<(int count, decimal reservedAmount)> GetReservedRequests(Guid ownerId);
        Task AddCredits(decimal amount, Guid userId);
    }
}
