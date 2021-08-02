using System;
using System.Collections.Generic;
using System.Text;

namespace IAutoM8.Service.ProjectTasks.Dto
{
    public class TaskChecklistDto : UpdateTaskChecklistDto
    {
        public string Name { get; set; }
        public int? ProjectTaskId { get; set; }
    }
}
