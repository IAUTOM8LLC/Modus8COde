using AutoMapper;
using IAutoM8.Global.Exceptions;
using IAutoM8.Repository;
using IAutoM8.Service.Client.Dto;
using IAutoM8.Service.Client.Interfaces;
using IAutoM8.Service.CommonService.Interfaces;
using IAutoM8.Service.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace IAutoM8.Service.Client
{
    public class ClientService : IClientService
    {
        private readonly IRepo _repo;
        private readonly ClaimsPrincipal _principal;
        private readonly IDateTimeService _dateTimeService;

        public ClientService(IRepo repo, ClaimsPrincipal principal, IDateTimeService dateTimeService)
        {
            _repo = repo;
            _principal = principal;
            _dateTimeService = dateTimeService;
        }

        public async Task<ClientDto> AddClientAsync(UpdateClientDto model)
        {
            var client = Mapper.Map<Domain.Models.Client.Client>(model);
            client.BusinessOwnerGuid = _principal.GetOwnerId();
            client.DateCreated = _dateTimeService.NowUtc;

            await _repo.AddAsync(client);
            await _repo.SaveChangesAsync();

            return Mapper.Map<ClientDto>(client);
        }

        public async Task DeleteClientAsync(int clientId)
        {
            using(var trx = _repo.Transaction())
            {
                var client = await trx.Track<Domain.Models.Client.Client>()
                    .FirstOrDefaultAsync(c => c.Id == clientId);

                if (client == null)
                    throw new ValidationException("Client is not found.");

                if (client.BusinessOwnerGuid != _principal.GetOwnerId())
                    throw new ForbiddenException("You have no access to client.");

                await trx.SaveChangesAsync();
                trx.RemoveRange(client.Projects);
                trx.Remove(client);

                await trx.SaveAndCommitAsync();
            }
        }

        public async Task<UpdateClientDto> GetClientAsync(int clientId)
        {
            var client = await _repo.Read<Domain.Models.Client.Client>()
                .FirstOrDefaultAsync(w => w.Id == clientId);

            if (client.BusinessOwnerGuid != _principal.GetOwnerId())
                throw new ForbiddenException("You have no access to skill.");

            return Mapper.Map<UpdateClientDto>(client);
        }

        public async Task<List<ClientDto>> GetClientsAsync()
        {
            var ownerGuid = _principal.GetOwnerId();
            return Mapper.Map<List<ClientDto>>(
                await _repo.Read<Domain.Models.Client.Client>()
                .Include(c => c.Projects)
                .Where(w => w.BusinessOwnerGuid == ownerGuid)
                .OrderBy(o => o.CompanyName)
                .ToListAsync());
        }

        public async Task<ClientDto> UpdateClientAsync(UpdateClientDto model)
        {
            var client = await _repo.Track<Domain.Models.Client.Client>()
                .Include(c => c.Projects)
                .FirstOrDefaultAsync(p => p.Id == model.Id);

            if (client == null)
                throw new ValidationException("Skill is not found.");

            if (client.BusinessOwnerGuid != _principal.GetOwnerId())
                throw new ForbiddenException("You have no access to skill.");

            Mapper.Map(model, client);
            client.LastUpdated = _dateTimeService.NowUtc;

            await _repo.SaveChangesAsync();

            return Mapper.Map<ClientDto>(client);
        }
    }
}
