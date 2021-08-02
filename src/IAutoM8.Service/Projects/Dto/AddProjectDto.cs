using System.Collections.Generic;

namespace IAutoM8.Service.Projects.Dto
{
    public class AddProjectDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public int? ClientId { get; set; }
        public int? ParentProjectId { get; set; }
        public List<ProjectUserDto> Managers { get; set; }
    }
}
