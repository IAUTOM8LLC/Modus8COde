using System;
using System.Collections.Generic;
using System.Text;

namespace IAutoM8.Service.ProjectTasks.Dto
{
    public class UpdateProjectNotesDto : AddProjectNotesDto
    {
        public int Id { get; set; }
        public bool IsPublished { get; set; }
    }
}
