using System.Collections.Generic;
using System.Threading.Tasks;
using IAutoM8.Service.Client.Dto;

namespace IAutoM8.Service.Client.Interfaces
{
    public interface IClientService
    {
        Task<List<ClientDto>> GetClientsAsync();
        Task<UpdateClientDto> GetClientAsync(int clientId);
        Task<ClientDto> AddClientAsync(UpdateClientDto model);
        Task<ClientDto> UpdateClientAsync(UpdateClientDto model);
        Task DeleteClientAsync(int clientId);
    }
}
