using System;
using System.Collections.Generic;

namespace IAutoM8.Service.ProjectTasks.Dto
{
    public class TaskFormulaDto
    {
        public int ProjectId { get; set; }
        public int FormulaId { get; set; }
        public int PosX { get; set; }
        public int PosY { get; set; }
        public DateTime StartDate { get; set; }
        public IEnumerable<SkillMapDto> SkillMappings { get; set; }
    }
}
