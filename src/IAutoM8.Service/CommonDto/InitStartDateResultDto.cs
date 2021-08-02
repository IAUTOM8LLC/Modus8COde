using IAutoM8.Domain.Models.Project.Task;
using System;
using System.Collections.Generic;

namespace IAutoM8.Service.CommonDto
{
    public class InitStartDateResultDto
    {
        public IEnumerable<ProjectTask> RootTasks { get; set; }
        public DateTime StartTime { get; set; }
        public int TotalDuration { get; set; }
    }
}
