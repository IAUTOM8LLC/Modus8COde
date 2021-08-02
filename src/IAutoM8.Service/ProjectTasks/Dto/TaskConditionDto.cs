using System.Collections.Generic;

namespace IAutoM8.Service.ProjectTasks.Dto
{
    public class TaskConditionDto
    {
        public string Condition { get; set; }
        public List<TaskConditionOptionDto> Options { get; set; }
    }
}
