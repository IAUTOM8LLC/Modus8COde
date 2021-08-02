using IAutoM8.InfusionSoft.Dto;
using System.Threading.Tasks;

namespace IAutoM8.InfusionSoft.Interfaces
{
    public interface IContactService
    {
        Task<int> AddAsync(ContactDto contactDto);

        Task<bool> ApplyRegisterActionAsync(int contactId);
        Task<bool> ApplyConfirmActionAsync(int contactId);
    }
}
