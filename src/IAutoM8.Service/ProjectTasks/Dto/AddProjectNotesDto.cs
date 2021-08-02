using System;
using System.Collections.Generic;
using System.Text;

namespace IAutoM8.Service.ProjectTasks.Dto
{
    public class AddProjectNotesDto
    {
        public string Text { get; set; }
        public int? FormulaId { get; set; }
        public int? ProjectId { get; set; }
        public int? ProjectTaskId { get; set; }
    }
}
