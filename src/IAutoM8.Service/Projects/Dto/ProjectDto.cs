using IAutoM8.Service.Resources.Dto;
using IAutoM8.Service.Users.Dto;
using System;
using System.Collections.Generic;

namespace IAutoM8.Service.Projects.Dto
{
    public class ProjectDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int? ClientId { get; set; }
        public string Client { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime? LastUpdated { get; set; }
        public List<ProjectUserDto> Managers { get; set; }
        public Guid OwnerGuid { get; set; }
        public OwnerDto Owner { get; set; }
        public List<ResourceDto> Resources { get; set; }
        public int? ParentProjectId { get; set; }
        public ProjectDto Parent { get; set; }
        public List<ProjectDto> Children { get; set; }
    }
}
