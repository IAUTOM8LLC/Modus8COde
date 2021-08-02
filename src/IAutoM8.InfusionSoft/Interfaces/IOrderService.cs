using IAutoM8.InfusionSoft.Responces;
using System.Threading.Tasks;

namespace IAutoM8.InfusionSoft.Interfaces
{
    public interface IOrderService
    {
        Task<int> PlaceOrderAsync(int contactId, int creditCardId, int payPlanId, int[] productIds,
            int[] subscriptionPlanIds, bool processSpecials, string[] promoCodes);
    }
}
