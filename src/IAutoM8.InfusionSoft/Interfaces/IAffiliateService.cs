using IAutoM8.InfusionSoft.Dto;
using System.Threading.Tasks;

namespace IAutoM8.InfusionSoft.Interfaces
{
    public interface IAffiliateService
    {
        Task<int> CreateReferralAsync(AffiliateDto affiliateDto);
        Task<AffiliateDto> GetReferralAsync(int contactId);
    }
}
