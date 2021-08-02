using System;
using System.Collections.Generic;
using System.Text;

namespace IAutoM8.Service.Projects.Dto
{
    public class AddChildProjectDto
    {
        public string Name { get; set; }
        public int? ChildProjectId { get; set; }
        public int? ParentProjectId { get; set; }
    }
}
