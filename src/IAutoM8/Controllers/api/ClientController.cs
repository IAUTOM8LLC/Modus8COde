using System.Collections.Generic;
using System.Threading.Tasks;
using IAutoM8.Controllers.Abstract;
using IAutoM8.Service.Client.Dto;
using IAutoM8.Service.Client.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IAutoM8.Controllers.api
{
    [Authorize(Roles = OwnerOrManager)]
    public class ClientController : BaseController
    {
        private readonly IClientService _clientService;
        public ClientController(IClientService clientService)
        {
            _clientService = clientService;
        }

        [HttpGet]
        public async Task<List<ClientDto>> Get()
        {
            return await _clientService.GetClientsAsync();
        }

        [HttpGet("{id}")]
        public async Task<UpdateClientDto> Get(int id)
        {
            return await _clientService.GetClientAsync(id);
        }
        
        [HttpPost]
        public async Task<ClientDto> Post([FromBody]UpdateClientDto model)
        {
            return await _clientService.AddClientAsync(model);
        }
        
        [HttpPut]
        public async Task<ClientDto> Put([FromBody]UpdateClientDto model)
        {
            return await _clientService.UpdateClientAsync(model);
        }

        [HttpDelete("{id}")]
        public async Task<OkResult> Delete(int id)
        {
            await _clientService.DeleteClientAsync(id);
            return Ok();
        }
    }
}
