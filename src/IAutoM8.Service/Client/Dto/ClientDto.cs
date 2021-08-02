
using System;

namespace IAutoM8.Service.Client.Dto
{
    public class ClientDto: UpdateClientDto
    {
        public DateTime DateCreated { get; set; }
        public DateTime? LastUpdated { get; set; }
        public bool HasAssignedProjects { get; set; }
    }
}
