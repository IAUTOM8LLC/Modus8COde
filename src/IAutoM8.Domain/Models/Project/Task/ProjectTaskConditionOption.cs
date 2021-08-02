using IAutoM8.Domain.Models.Abstract.Task;
using IAutoM8.Global.Enums;
using System.Collections.Generic;

namespace IAutoM8.Domain.Models.Project.Task
{
    public class ProjectTaskConditionOption
        : TaskConditionOption<ProjectTaskCondition, ProjectTask>
    {
        public virtual ICollection<TaskHistory> TaskHistories { get; set; } = new List<TaskHistory>();
    }
}
