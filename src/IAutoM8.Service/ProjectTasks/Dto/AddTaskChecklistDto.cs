using System;
using System.Collections.Generic;
using System.Text;

namespace IAutoM8.Service.ProjectTasks.Dto
{
    public class AddTaskChecklistDto
    {
        public int Type { get; set; }
        public List<string> Milestones { get; set; } = new List<string>();
    }
}
