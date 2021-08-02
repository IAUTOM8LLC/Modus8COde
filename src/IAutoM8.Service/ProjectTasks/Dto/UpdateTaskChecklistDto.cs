using System;
using System.Collections.Generic;
using System.Text;

namespace IAutoM8.Service.ProjectTasks.Dto
{
    public class UpdateTaskChecklistDto
    {
        public int Id { get; set; }
        public int Type { get; set; }
        public bool? TodoIsChecked { get; set; }
        public bool? ReviewerIsChecked { get; set; }
    }
}
